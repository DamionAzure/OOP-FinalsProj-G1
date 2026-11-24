Animal sound effects for the Terminal.Gui interface (optional). Files must be .wav.

Name a sound after the animal's ID or its common name. For ID A001 / common name Lion:

    A001.wav        ID sound
    A001-1.wav      ID sound, numbered variant (any number: -1, -2, -3 ... gaps are fine)
    A001-2.wav      ID sound, numbered variant
    Lion.wav        common-name sound ("Philippine Tarsier" also matches Philippine_Tarsier / Philippine-Tarsier)

"Play Sound" button (F5) is purely random:
  - Finds every ID sound automatically (A001.wav, A001-1.wav, ...) and picks one at random.
  - The common-name sound (Lion.wav) is picked 25% of the time; otherwise an ID sound is picked.
  - If there is no common-name sound, an ID sound is always picked.
  - If there are no ID sounds, the common-name sound is used.
  - Only files that exist are ever chosen.

Cooldown: after the button is used it is disabled for 2 seconds (a countdown is shown), even if you
switch to another animal.

Folders searched: ./Audio (where you run from; new sounds appear without rebuilding) and the Audio folder
next to the built program. Playback uses Windows' built-in player, afplay on macOS, and paplay / aplay /
ffplay / play (whichever is installed) on Linux. No extra packages are needed.

Use only sounds you recorded yourself or that are licensed for reuse, and cite them in the report's References.
