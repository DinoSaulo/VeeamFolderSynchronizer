# VeeamFolderSynchronizer
## Test Task

Please implement a program that synchronizes two folders: source and replica. The program should maintain a full, identical copy of source folder at replica folder. Solve the test task by writing a program in C#.

- Synchronization must be one-way: after the synchronization content of the replica folder should be modified to exactly match content of the source folder;

- Synchronization should be performed periodically;

- File creation/copying/removal operations should be logged to a file and to the console output;

- Folder paths, synchronization interval and log file path should be provided using the command line arguments;

- It is undesirable to use third-party libraries that implement folder synchronization;

- It is allowed (and recommended) to use external libraries implementing other well-known algorithms. For example, there is no point in implementing yet another function that calculates MD5 if you need it for the task – it is perfectly acceptable to use a third-party (or built-in) library. 

### Usage
Using the CMD or PowerShell, inside the folder VeeamFolderSynchronizer, execute the command below:
```bash
    dotnet run --sourcePath=[sourcePath]  --replicaPath=[replicaPath] --timeInterval=[timeInterval] --logPath=[logPath]
```

#### Example command to execute

```bash
    run --sourcePath=C:\\sourcePath --replicaPath=C:\\replicaPath --timeInterval=1 --logPath=C:\\logPath
```

### Parameters description
| Command | Description |
| --- | --- |
|sourcePath     | Root path that will be synchronized                      |
|replicaPath    | Path to the folder that will receive the modifications    |
|timeInterval   | Time in minutes to synchronize the folder again           |
|logPath        | Path what will register the logs of synchronizer execution|

