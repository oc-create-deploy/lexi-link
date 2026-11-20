# Native acceptance

Runtime844c1d8, simulator export44661456694, native QA44662777815 passed.
Actual UI gestures entered four Russian words using optional tap-letter + submit
input, rejected invalid input, completed level one, and restored level two after
process relaunch. A real word hint reduced remaining words and its count survived
another cold launch. Default drag received a single-letter smoke gesture only;
full multi-letter drag completion was not exercised. Tap input is a permanent
user-facing accessibility option, not a test-only model-control path.

Reviewed hint screenshot: word revealed, hint count1, readable letters, safe layout.
Content remains Russian word puzzles, with English guidance; not English vocabulary.
No physical device available. TestFlight build/group/tester verification remains.
