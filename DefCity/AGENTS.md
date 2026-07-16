# Agent Guide

## Scope
- Primary gameplay work currently lives under `Assets/Scripts/ElementalDefense`.
- Shared core defense systems are reused from DefCity, but ElementalDefense is the active development focus.

## Project Priorities
- Favor correctness and debuggability over defensive fallbacks during development.

## Project Intent
- ElementalDefense is a simpler, more complete game built to refine the shared defense systems that will also support DefCity.
- DefCity remains the broader parent project, while ElementalDefense is the main code path for day-to-day development.

## Code Organization
- Prefer direct, readable code over premature abstraction.