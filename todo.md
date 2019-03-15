# TODO

## .nyokaremote

rename file to .nyokaremote.json

## class NyokaRemote

    this class should be shared with everybody who wants to access .nyokaremote.json

* add method Save()

## class NyokaRemoteInfo

* as singleton

## documentation

update all docu (when typed incomplete commands)

i.e. `nyoka publish`  
or `nyoka`

## -- and - options

* `nyoka publish -r ...`
* `nyoka publish --repository ...`

## write unit tests

## create subproject

containing

* NyokaRemote
* NyokaRemoteInfo

## add verb deploy

to move (stage) code, data, and models from the local environment to either a ZementisModeler or a ZementisServer

## restriction for ZementisServer

    for deploy and add use only file types .pmml, .jar