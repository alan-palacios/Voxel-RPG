using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequested : MonoBehaviour
{
  static ThreadedDataRequested instance;
  Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

  void Awake(){
    instance=FindObjectOfType<ThreadedDataRequested>();

  }
  public static void RequestData( Func<object> generateData, Action<object> callback) {
		ThreadStart threadStart = delegate {
			instance.DataThread (generateData, callback);
		};

		new Thread (threadStart).Start ();
	}

	void DataThread(Func<object> generateData, Action<object> callback) {
              object data = generateData();
              lock (dataQueue) {
          			dataQueue.Enqueue (new ThreadInfo (callback, data));
          		}
	}

	void Update() {
              while (dataQueue.Count > 0) {
                  lock(dataQueue){
                            ThreadInfo threadInfo = dataQueue.Dequeue ();
                            threadInfo.callback (threadInfo.parameter);
                  }
              }
	}

  struct ThreadInfo {
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo (Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

	}
}
