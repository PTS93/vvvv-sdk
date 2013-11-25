using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel;

using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;

using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

namespace HttpRequestNode
{

	public enum HttpRequestMethods
	{
		GET,
		POST,
		PUT,
		DELETE,
		HEAD,
		TRACE,
		OPTION
	}

    public enum HttpEncoding
    {
        ANSI,
        UTF8,
        ASCI
    }




	public class RequestState
	{
		// This class stores the State of the request.

		const int BUFFER_SIZE = 1024;
		public byte[] BufferRead = new byte[BUFFER_SIZE];

		public StringBuilder RequestData = new StringBuilder("");
		public HttpWebRequest Request = null;
		public HttpWebResponse Response = null;
		public Stream StreamResponse = null;

		public int SliceId = -1;
		public string Content = String.Empty;
		public string StatusMessage = String.Empty;
		public string Url = String.Empty;
		public string Accept = String.Empty;
		public HttpRequestMethods RequestMethod;
		public string ContentType = String.Empty;
		public int Timeout = 1000;
		public bool Aborted = false;

        public ManualResetEvent ManulResetEvent = new ManualResetEvent(false);
		public List<Exception> Exceptions = new List<Exception>();
	}


	[PluginInfo(Name = "HTTP",
	            Category = "Network",
	            Version = "Request",
	            Tags = "", Author = "phlegma",
	            AutoEvaluate = false
	           )]
	public class HttpRequestNode : IPluginEvaluate
	{
		#region vvvv declaration
		[Import()]
		ILogger FLogger;

		[Input("URL")]
		IDiffSpread<string> FUrlIn;

		[Input("Request")]
		IDiffSpread<bool> FSendRequestIn;

		[Input("Request Method", DefaultEnumEntry = "GET")]
		IDiffSpread<HttpRequestMethods> FRequestMethodIn;

		[Input("Content")]
		IDiffSpread<string> FContentIn;

		[Input("Content Type", DefaultString = "text/plain")]
		IDiffSpread<string> FContentTypeIn;

		[Input("Accept", DefaultString = "*/*")]
		IDiffSpread<string> FAcceptIn;

		[Input("Timeout", DefaultValue=10000)]
		ISpread<int> FTimoutIn;

        [Input("Encoding", EnumName = "HttpEncoding")]
        IDiffSpread<EnumEntry> FEncoding;

		[Output("Response")]
		ISpread<string> FResponseOut;

		[Output("Statuscode")]
		ISpread<string> FStatusCodeOut;

		[Output("Fail", IsBang=true)]
		ISpread<bool> FFailOut;

		[Output("Success", IsBang=true)]
		ISpread<bool> FSuccessOut;

        [Output("Active")]
        ISpread<bool> FActiveOut;

		[Output("Elapse Time")]
		ISpread<double> FElaspeTimeOut;


		[ImportingConstructor]
        public HttpRequestNode()
		{ 
			var s = new string[]{"Ansi","Ascii","UTF8", "UTF32","Unicode"};
			//Please rename your Enum Type to avoid 
			//numerous "MyDynamicEnum"s in the system
		    EnumManager.UpdateEnum("HttpEncoding", "Ansi", s);  
		}

		#endregion

		

		#region field declaration
		
		const int BUFFER_SIZE = 1024;

        


		List<bool> FChangedSlice = new List<bool>();
		ISpread<string> FLastUrls;

		
		List<int> FSuccess = new List<int>();
		List<int> FFail = new List<int>();
		int FCounter = 0;

		Dictionary<int, string> FHttpStatusCode = new Dictionary<int, string>();
		Dictionary<int, RequestState> FRequestStateList = new Dictionary<int, RequestState>();
		Dictionary<int, BackgroundWorker> FThreadList = new Dictionary<int, BackgroundWorker>();
		

		#endregion


		#region IPluginEvaluate Members

		public void Evaluate(int SpreadMax)
		{

			FResponseOut.SliceCount = SpreadMax;
			FFailOut.SliceCount = SpreadMax;
			FSuccessOut.SliceCount = SpreadMax;
			FStatusCodeOut.SliceCount = SpreadMax;
			FActiveOut.SliceCount = SpreadMax;
			FElaspeTimeOut.SliceCount = SpreadMax;


			#region Make a async request

			if (FSendRequestIn.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FSendRequestIn[i])
					{

                        if (FThreadList.ContainsKey(i))
                        {

                            BackgroundWorker worker;
                            RequestState RequestStateDelete;
                            FThreadList.TryGetValue(i, out worker);
                            FRequestStateList.TryGetValue(i, out RequestStateDelete);
                            RequestStateDelete.Request.Abort();
                            RequestStateDelete.ManulResetEvent.Set();
                            worker.CancelAsync();

                            FThreadList.Remove(i);
                            FRequestStateList.Remove(i);
                        }
						
						BackgroundWorker Worker = new BackgroundWorker();
						Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
						Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
						
						Worker.WorkerSupportsCancellation = true;
						
						RequestState RequestState = new RequestState();
						RequestState.Accept = FAcceptIn[i];
						RequestState.Content = FContentIn[i];
						RequestState.RequestMethod = FRequestMethodIn[i];
						RequestState.ContentType = FContentTypeIn[i];
						RequestState.Url = FUrlIn[i];
						RequestState.Timeout = FTimoutIn[i];
						
						RequestState.SliceId = i;
						
						Worker.RunWorkerAsync(RequestState);
                        FActiveOut[i] = true;
						FThreadList.Add(i,Worker);
                        FRequestStateList.Add(i, RequestState);
					}
				}
			}

			#endregion



            for (int i = 0; i < SpreadMax; i++)
            {
                FSuccessOut[i] = false;
                FFailOut[i] = false;
            }
		}
		
		
		#region Backrounworker Handlers

		void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
            RequestState RequestState = e.Result as RequestState;

			if ((e.Cancelled == true))
			{
                
				FLogger.Log(LogType.Message,"Request was cancelled");
			}
			else if (!(e.Error == null))
			{
				FLogger.Log(e.Error);
                
			}
			else
			{
				Debug.WriteLine(e.Result.ToString());
				//RequestState RequestState = e.Result as RequestState;
				
				int SliceId = RequestState.SliceId;
				

				if(RequestState.Exceptions.Count > 0)
				{
					foreach(Exception Ex in RequestState.Exceptions)
					{
						FLogger.Log(LogType.Error,Ex.Message);
                        
					}
                    RequestState.Exceptions.Clear();
                    FFailOut[SliceId] = true;
				}else
				{
					if(RequestState.RequestData.Length > 0)
						FResponseOut[SliceId] = RequestState.RequestData.ToString();
                    FSuccessOut[SliceId] = true;
				}
				
				try
				{
					FStatusCodeOut[SliceId] = RequestState.Response.StatusCode.ToString();
				}catch(NullReferenceException ex)
				{
                    FFailOut[SliceId] = true;
                    FLogger.Log(LogType.Error, "No Response received");
				}

				FActiveOut[SliceId] = false;
			}
		}

		void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;
			RequestState RequestState =  e.Argument as RequestState;
			
			if ((worker.CancellationPending == true))
			{
				e.Cancel = true;
			}
			else
			{
				int Index = RequestState.SliceId;
				
				Uri Uri = new Uri(RequestState.Url);
				HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(Uri);
				Request.Method = RequestState.RequestMethod.ToString();
				Request.ContentType = RequestState.ContentType;
				Request.Accept = RequestState.Accept;
				
				RequestState.Request = Request;
				IAsyncResult Result = null;
				
				switch (RequestState.RequestMethod)
				{
					case HttpRequestMethods.GET:
                        Result = (IAsyncResult)Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
						break;

					case HttpRequestMethods.POST:

						Request.ContentLength = FContentIn[Index].Length;
						Result = (IAsyncResult)Request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), RequestState);
						break;

					case HttpRequestMethods.HEAD:
						Result = (IAsyncResult)Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
						break;

					case HttpRequestMethods.PUT:
						Request.ContentLength = FContentIn[Index].Length;
						Result = (IAsyncResult)Request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), RequestState);
						break;

					case HttpRequestMethods.DELETE:
						Result = (IAsyncResult)Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
						break;

					case HttpRequestMethods.TRACE:
						Result = (IAsyncResult)Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
						break;

					case HttpRequestMethods.OPTION:
						Result = (IAsyncResult)Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
						break;
				}

             	ThreadPool.RegisterWaitForSingleObject (Result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), RequestState, RequestState.Timeout, true);
                if(!RequestState.Aborted)
                {
                    RequestState.ManulResetEvent.WaitOne();
                    
                }

                if (RequestState.Request.HaveResponse)
                {
          
                        RequestState.Response.Close();
                    
                }
                    

                e.Result = RequestState;
			}
			
		}
		
		
		#endregion


		private void GetRequestStreamCallback(IAsyncResult Result)
		{
			int Slice = 0;
			RequestState RequestState = (RequestState)Result.AsyncState;
			Slice = RequestState.SliceId;
            
			try
			{
				HttpWebRequest Request = RequestState.Request;
				// End the operation
				Stream postStream = Request.EndGetRequestStream(Result);


				// Convert the string into a byte array.
				byte[] byteArray = Encoding.Default.GetBytes(RequestState.Content);

				// Write to the request stream.
				postStream.Write(byteArray, 0, RequestState.Content.Length);
				postStream.Close();

				// Start the asynchronous operation to get the response
				Request.BeginGetResponse(new AsyncCallback(ResponseCallback), RequestState);
			}
			catch (WebException e)
			{
				RequestState.Exceptions.Add(e);
			}

            RequestState.ManulResetEvent.Set();
		}

		

		private void ResponseCallback(IAsyncResult Result)
		{
			int Slice = 0;
			RequestState RequestState = (RequestState)Result.AsyncState;
			Slice = RequestState.SliceId;

			try
			{
				// State of request is asynchronous.

				HttpWebRequest HttpWebRequest = RequestState.Request;
				RequestState.Response = (HttpWebResponse)HttpWebRequest.EndGetResponse(Result);

				// Read the response into a Stream object.
				Stream responseStream = RequestState.Response.GetResponseStream();
				RequestState.StreamResponse = responseStream;

				// Begin the Reading of the contents of the HTML page and print it to the console.
				IAsyncResult asynchronousInputRead = responseStream.BeginRead(RequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), RequestState);
				return;
			}
			catch (WebException e)
			{
				RequestState.Exceptions.Add(e);
			}

            RequestState.ManulResetEvent.Set();
		}


		private void ReadCallBack(IAsyncResult asyncResult)
		{
			int Slice = 0;
			RequestState RequestState = (RequestState)asyncResult.AsyncState;
			Slice = RequestState.SliceId;

			try
			{

				Stream responseStream = RequestState.StreamResponse;

				int read = responseStream.EndRead(asyncResult);
				// Read the HTML page and then print it to the console.
				if (read > 0)
				{
                    switch (FEncoding[Slice].Index)
                    {
                        case (0):
                            RequestState.RequestData.Append(Encoding.Default.GetString(RequestState.BufferRead, 0, read));
                            break;
                        case (1):
                            RequestState.RequestData.Append(Encoding.ASCII.GetString(RequestState.BufferRead, 0, read));
                            break;
                        case (2):
                            RequestState.RequestData.Append(Encoding.UTF8.GetString(RequestState.BufferRead, 0, read));
                            break;
                        case (3):
                            RequestState.RequestData.Append(Encoding.UTF32.GetString(RequestState.BufferRead, 0, read));
                            break;
                        case (4):
                            RequestState.RequestData.Append(Encoding.Unicode.GetString(RequestState.BufferRead, 0, read));
                            break;
                        default:
                            RequestState.RequestData.Append(Encoding.Default.GetString(RequestState.BufferRead, 0, read));
                            break;
                    }

					IAsyncResult asynchronousResult = responseStream.BeginRead(RequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), RequestState);
					return;
				}
				else
				{
					if (RequestState.RequestData.Length >= 0)
					{
						responseStream.Close();

					}
				}

			}
			catch (WebException e)
			{
				RequestState.Exceptions.Add(e);
			}
			catch (ObjectDisposedException ex)
			{
				RequestState.Exceptions.Add(ex);
			}

            RequestState.ManulResetEvent.Set();
		}


		// Abort the request if the timer fires.
		private  void TimeoutCallback(object state, bool timedOut)
		{
			if (timedOut)
			{
				RequestState RequestState = state as RequestState;
				RequestState.Aborted = true;
			}
		}

	}
	#endregion

}
