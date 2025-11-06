science.ros:
- gather what you do not have in containers (optional by button but default)
- auto restore experiments (whenever it can be restored = eva scientist close enough)

control.ros:
- UTL instead of MAN (doing auto-exec on top of NDE) for other utilities/tools/actions/settings
- auto-close panels and antena when entering atmosphere
- auto-deploy panels and antena when exiting atmosphere
  - when not shielded and not marked noauto... so, deploy not-noauto fairings as well
  - maybe some global noauto on root part
- enhance landing (currently only srf:retro throttle limiter)
  - auto throttle-up when necessary (and maybe 1% as a signal of danger)
  - buttons for adjusting landing zone: plus and minus 10, 20, 40 degrees in radial
    - you currently have to fight PIDs with user input

advertise (if you feel like to):
- list major features/benefits
  - autopilot (without any part)
  - can control nearby ship as well (e.g. station you are docking to)
  - access to reflected native API and objects
    - very helpful to learn KSP API and how to create mods
    - can also be used in scripts, even in VAB/SPH, to calculate deltaV or whatever
