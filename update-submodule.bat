git submodule foreach --recursive git submodule update --init

git -c diff.mnemonicprefix=false -c core.quotepath=false fetch origin

git -c diff.mnemonicprefix=false -c core.quotepath=false pull origin master

git -c diff.mnemonicprefix=false -c core.quotepath=false submodule update --init --recursive
