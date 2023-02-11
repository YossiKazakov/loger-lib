**Part I - Producer Consumer**
------------
***What caused the problem?***

The LoggerLib class has a single string `m_message` to store the current message that needs to be printed.

If CPD < SPD then `m_message` variable can be changed more than once by the client before the
server gets to print it (which can causes an incomplete printing such as "0, 2, 5 ,7....").

if CPD > SPD then the same `m_message` variable will be printed multiple times.

***What is the solution?***

Since this problem is eventually a producer/consumer problem, the right thing to do is to store
the messages in a Queue<string> collection, or even better, ConcurrentQueue<string>.
The producer (`PrintLogLine` method) will then enqueue to this queue and the consumer (`DoWork` method), on signal, will
dequeue from it.

Notice that a locking mechanism is needed only when consuming (to avoid a situation where more then one thread is
dequeuing and writing at the same time, which can ruin the correct ordering), and not when enqueuing (since the main
thread is calling the method in a synchronous way).

***And yet, another problem***

Now the problem is that if CPD < SPD then the main thread will terminate (itself and all the tasks that are running,
since tasks are background threads)
before the server managed to write all the messages in the queue.

***Final solution***

To solve it, we need some mechanism to wait until the queue is empty.
I decided to create a method `SignalIfWorkIsDone` which is called at each `DoWork` iteration. The work is done if the
queue is empty. This method which waits for this signal is `WaitForThePrintsToFinish`, which is called in the main
thread, after the for-loop.

This way, all cases are handled:
If CPD < SPD then the queue grows faster then it is emptying, so the signal will fire only when the work is done.
If CPD > SPD then the queue empties faster then it grows, and the signal will fire before the work is done, but the
method that waits for it (`WaitForThePrintsToFinish`) will be called only after the work is done (after the for-loop).

***Notes***

1) In **CPD** I mean **Client Printing Delay** and in **SPD** I mean **Server Printing Delay**. When CPD < SPD the
   client produce faster then the server consumes and when SPD < CPD the server consumes faster then the client produce.
2) When CPD = SPD the solution still holds.
3) The second problem could be "solved" just by adding `Console.ReadKey()` at the end `Main`.

***Modification Summary***

1) `string m_message` field changed to `ConcurrentQueue<string> _messages`
2) `object _writingLock` for locking the writing
3) `AutoResetEvent _readyToWriteSignal` for signaling when there is something to write
4) `AutoResetEvent _workDoneSignal` for signaling when the work is done (the queue is empty)
5) `private void DequeueAndWriteMessages()` for readability inside the `DoWork()` method
6) `void WaitForThePrintsToFinish()` to the `ILogLib` interface
7) Server Printing Delay moved from `DoWork()` to `FileWriter` class

**Part II - Filtering Option**
-------------

***Modification Summary***

1) `Filters` directory
2) `IlogFilter` interface which agrees on a single method `string Filter(string messageToFilter)` that takes a given log
   and returns a modified version of it
3) Specific filter classes that implements the above interface, such as:
    * `AddDateFilter` that adds the current time and date at the beginning of the given log
    * `ReplaceRegexFilter` that takes in a regex pattern and a string to replace it and returns the result
    * `RemoveSpecialCharacters` which is just a private case of the above class (so it inherits from it)
4) An option to initialize the `LogerLib` with a list of filters (`List<ILogFilter>`)
5) `private readonly List<ILogFilter>? _logFilters` field in `LogerLib` for storing the above list
6) `private string ApplyFilters(string message)` method that iterates through the `List<ILogFilter>` and applies each
   filter on the given message

Part III - Unit Testing
-------------------------

***Steps I have taken***

1) In order to somehow compare between the desired output to the actual, I created an internal `TestFileWriter` class that stores the logs it is given to a list
2) In the main [TestClass] I wrote `List<string> SetupTestAndReturnResults(int serverPrintingDelay = 0,
   int clientPrintingDelay = 0, List<string> messagesToActOn = null,
   List<ILogFilter> filters = null)` method which encapsulates the whole testing logic: 
   * **Arrange**  
      *  Creates TestFileWriter instance with the desired server printing delay given by parameter
      *  Creates LoggerLib instance with the nullable list of filters given by a parameter
      *  If the list of messages to act on is not given by parameter, it uses the default one ("0", "1", "2",...)
   * **Act** 
     * For each of the messages in `messagesToActOn` it calls `logger.PrintLogLine` and sleeps `clientPrintingDelay` milliseconds
   * **Return result for future assertion**
      * Returns `fileWriter.LogOutputList` which is where the logs that were "printed" are stored
3) I wrote `DifferentDelayRatioTests` region which contains few [TestMethod]'s, each of them simulates different ratio between the CPD and SPD and checks that everything is printed in the correct order
4) I wrote `FilterTests` region which for now contains only one method that checks whether the `RemoveSpecialCharactersFilter` applied correctly

**Notes**
* I tried as much as I can to follow the DRY and DAMP principles
* I used the MethodTested_Scenario_Expected naming conventions























