"""Drive normal native touch input; QA snapshots are read-only observations."""
import json, os, pathlib, subprocess, time
out = pathlib.Path('screenshots'); out.mkdir(exist_ok=True)
udid = os.environ['UDID']; bundle = os.environ['APP_BUNDLE_ID']
container = pathlib.Path(subprocess.check_output(['xcrun','simctl','get_app_container',udid,bundle,'data'],text=True).strip())
seq = 0
import importlib.util
spec=importlib.util.spec_from_file_location('touchbridge',pathlib.Path(__file__).with_name('native-touch-bridge.py'))
module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
bridge=module.TouchBridge(out,udid,bundle)

def state():
    deadline = time.time()+90
    while time.time()<deadline:
        for p in container.rglob('qa-state.json'):
            try:
                s=json.loads(p.read_text())
                assert not s['errors'],s['errors']
                return s
            except (json.JSONDecodeError,FileNotFoundError): pass
        time.sleep(.5)
    probe_errors=list(container.rglob('qa-recorder-error.txt'))
    if probe_errors:
        detail=probe_errors[0].read_text()
        (out/'qa-recorder-error.txt').write_text(detail)
        raise AssertionError('Native diagnostic recorder failed: '+detail)
    raise AssertionError('Read-only native snapshot missing')

def capture(label):
    (out/(label+'.json')).write_text(json.dumps(state(),indent=2))
    subprocess.run(['xcrun','simctl','io',udid,'screenshot',str(out/(label+'.png'))],check=True)

def point(item):
    assert 0<item['x']<1 and 0<item['y']<1,item
    return f"{round(item['x']*100)}%, {round(item['y']*100)}%"

def swipe_point(item):
    return f"{item['x']*100:.4f}%, {item['y']*100:.4f}%"

def action(body):
    global seq
    seq+=1
    p=out/f'input-{seq:03}.yaml'
    p.write_text(f'appId: {bundle}\n---\n'+body+'\n')
    bridge.action(body)
    time.sleep(1)

def tap(text):
    choices=[i for i in state()['items'] if i['kind']=='button' and i['interactable'] and text.casefold() in (i.get('text','').strip().casefold(),i['name'].strip().casefold())]
    assert choices,('Button missing',text)
    action('- tapOn:\n    point: "'+point(choices[0])+'"')

def value(key):return next(v['value'] for v in state()['values'] if v['key']==key)
def wait_for(predicate,label):
    end=time.time()+90
    while time.time()<end:
        if predicate():return
        time.sleep(1)
    capture('failure-'+label);raise AssertionError(label)
def xy(x,y):action('- tapOn:\n    point: "'+point({'x':x,'y':y})+'"')
def gui(x):
    s=state();scale=s['width']/430
    top=s['height']-s['safeY']-s['safeH']+27*scale
    xy(x/430,top/s['height'])
def enter(word):
    for char in word:
        item=next(i for i in state()['items'] if i['kind']=='letter' and i['text']==char)
        xy(item['x'],item['y'])
    gui(267);time.sleep(2)
def remaining():return int(value('SquareManager._numOfWords'))
capture('01-startup');xy(.5,.55);time.sleep(3);xy(.5,.55);time.sleep(3)
capture('02-tutorial-dismissed')
assert int(value('PlayerManager.GetCurrentLevelNum'))==1,'Not a fresh first level'
# Default drag still receives real input: single-letter gesture cannot solve a word.
letters=[i for i in state()['items'] if i['kind']=='letter'];n=remaining()
action('- swipe:\n    start: "'+point(letters[0])+'"\n    end: "50%, 58%"\n    duration: 3000')
assert remaining()==n,'Single-letter drag changed puzzle'
gui(105);wait_for(lambda:value('LetterInput.tapMode')=='True','tap-mode')
enter('АТС');assert remaining()==n,'Invalid word changed remaining count'
capture('03-invalid-word')
for word in ['ЛИСА','АИСТ','ЛИСТ','СИЛА']:
    before=remaining();enter(word)
    wait_for(lambda:remaining()==before-1,'accepted-'+word)
    capture('04-word-'+str(before))
wait_for(lambda:int(value('PlayerManager.GetCurrentLevelNum'))==2,'level-complete')
capture('05-complete')
# Relaunch must preserve completed progression, then test and persist real word hint.
subprocess.run(['xcrun','simctl','terminate',udid,bundle],check=True)
for p in container.rglob('qa-state.json'):p.unlink()
subprocess.run(['xcrun','simctl','launch',udid,bundle,'--playabilityQA'],check=True)
wait_for(lambda:int(value('PlayerManager.GetCurrentLevelNum'))==2,'progress-restored')
xy(.5,.55);time.sleep(3)
hints=int(value('PlayerManager.GetHintWordAmount'));before=remaining();tap('WordHintButton')
wait_for(lambda:int(value('PlayerManager.GetHintWordAmount'))==hints-1 and remaining()==before-1,'word-hint')
capture('06-hint')
subprocess.run(['xcrun','simctl','terminate',udid,bundle],check=True)
for p in container.rglob('qa-state.json'):p.unlink()
subprocess.run(['xcrun','simctl','launch',udid,bundle,'--playabilityQA'],check=True)
wait_for(lambda:int(value('PlayerManager.GetHintWordAmount'))==hints-1,'hint-persisted')
capture('07-restored')
(out/'result.json').write_text(json.dumps({'fourWordsAccepted':True,'invalidWordRejected':True,'dragInputSmoke':True,'optionalTapInput':True,'levelProgressPersisted':True,'hintUsedAndPersisted':True},indent=2))
