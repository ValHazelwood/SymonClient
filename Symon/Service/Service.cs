using System.ServiceProcess;

namespace Symon
{
	/// <summary>
	/// Mail server windows service.
	/// </summary>
	public class SymonService : ServiceBase
	{
        private DataSender m_pServer = null;
        
        /// <summary>
		/// Default constructor.
		/// </summary>
		public SymonService()
		{
			m_pServer = new DataSender();
            
            this.ServiceName = SymonMain.serviceName;
		}

		#region method OnStart
                
		/// <summary>
		/// Is called by OS when service must be started.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		protected override void OnStart(string[] args)
		{ 
            m_pServer.Start();
		}

		#endregion

		#region method OnStop
 		
		/// <summary>
		/// Is called by OS when service must be stopped.
		/// </summary>
		protected override void OnStop()
		{			
			m_pServer.Stop();
		}

		#endregion
			
	}
}
