﻿# CriticalDice

## Changelog

### 1.4.0

  * Updated for `v0.214.2` patch.

### 1.3.0

  * Create this `README.md` and backfill it with commit messages.
  * Fix dice result response broken by in `v0.211.7` patch due to `RPC_Say` adding an extra argument.

### 1.2.0

  * Modified CriticalDice to be dependent on BetterZeeRouter as it can no longer use a transpiler..
  * Modified the SayHandler to extract the necessary params and pass them forward as arguments.

### 1.1.1

  * Fixed a bug in CriticalDice regex for simple number so that having a trailing space is optional.

### 1.1.0

  * Rewrote the entire mod to run within an Coroutine and async Task to offload from the main network thread.
  * Bypass calling ZRoutedRpc.RouteRPC() and create the package data directly and queue it onto each RPC.  

### 1.0.0

  * Initial release.