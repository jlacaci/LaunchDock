@echo off
title LaunchDock - Publicar
cd /d "%~dp0.."
powershell.exe -ExecutionPolicy Bypass -File "%~dp0Publish.ps1" %*
