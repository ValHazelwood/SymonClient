/*
 * Copyright (c) 20011-2013 Val Hazelwood
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *    - Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    - Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials provided
 *      with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *
 */

using System.ServiceProcess;

namespace Symon
{
	/// <summary>
	/// Symon windows service.
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
