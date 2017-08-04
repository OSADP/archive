#!/bin/sh

svn rm --force ./oncoming
svn rm --force ./responder

svn commit -m "removing build folders before performing a new build".

rm -rf ./oncoming
mkdir ./oncoming
rm -rf ./responder
mkdir ./responder

make clean all -C ../source/OBU
cp ../bin/OBU_tchain2.0/rescume ./oncoming/.
cp ../bin/OBU_tchain2.0/rescume ./responder/.

make oncoming -C ../source/OBU
rsync -avC ../source/OBU/resources/* ./oncoming/.

make responder -C ../source/OBU
rsync -avC ../source/OBU/resources/* ./responder/.

svn add *
