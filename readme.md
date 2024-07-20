# Goal

Make a WebGL version playable at https://Feddas.github.io/Clawful

# Maintence

When publishing a new web build please amend the previous commit. This will clobber the previous webGL build. This should be done as otherwise this branch will quickly get bloated due to webassembly files.

# Unity

If you see the error:
```
Unable to parse Build/WebGL.framework.js.br!
If using custom web server, verify that web server is sending .br
files with HTTP Response Header "Content-Encoding: br". Brotli
compression may not be supported over HTTP connections. Migrate
your server to use HTTPS.
```
it means you need to turn off compression. Build / `Player settings` > `Publishing Settings` > `Compression Format` > `Disabled` (Not `Brotli`)

# Ophans

This is an orphan branch. As described on https://gitlab.com/tortoisegit/tortoisegit/-/issues/1090 , it can be created with TortoiseGit using:
1. create an new empty repo somewhere else.
2. immediately switch/checkout to new branch "gh-pages".
3. commit only to that new branch instead of master.
4. push the new branch to the remote repo.
5. wait for github.com to build the page. Status can be checked on https://github.com/Feddas/Clawful/settings/pages.
6. fetch from the remote repo in your old repo.

# Repo

https://github.com/Feddas/Clawful/tree/gh-pages
