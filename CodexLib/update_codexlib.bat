@echo off

cd lib

curl -s -H "Accept: application/vnd.github.VERSION.sha" "https://api.github.com/repos/Truinto/DarkCodex/commits/master" >check.commit

fc /b check.commit latest.commit >nul

if errorlevel 1 (
	echo Downloading CodexLib...
	curl -LOs "https://github.com/Truinto/DarkCodex/raw/master/CodexLib/CodexLib.dll"
	curl -LOs "https://github.com/Truinto/DarkCodex/raw/master/CodexLib/CodexLib.xml"
	move "check.commit" "latest.commit" >nul
) else (
	echo CodexLib is up to date
	del check.commit
)

pause
