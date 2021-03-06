﻿using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
   public class FailoverHandoverInvoker: IClusterInvoker
    {
        #region Field
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;
        private readonly IServiceCommandProvider _commandProvider;
        #endregion Field

        #region Constructor

        public FailoverHandoverInvoker(IRemoteInvokeService remoteInvokeService, IServiceCommandProvider commandProvider,
            ITypeConvertibleService typeConvertibleService, IBreakeRemoteInvokeService breakeRemoteInvokeService)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _breakeRemoteInvokeService = breakeRemoteInvokeService;
            _commandProvider = commandProvider;
        }

        #endregion Constructor

        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string _serviceKey)
        {
            var time = 0;
            T result = default(T);
            RemoteInvokeResultMessage message = null;
            var command =await _commandProvider.GetCommand(serviceId);
            do
            {
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey);
                if (message != null && message.Result != null)
                    result = (T)_typeConvertibleService.Convert(message.Result, typeof(T));
            } while (message == null && ++time < command.FailoverCluster);
            return result;
        }

        public async Task Invoke(IDictionary<string, object> parameters, string serviceId, string _serviceKey)
        {
            var time = 0;
            var command =await _commandProvider.GetCommand(serviceId);
            while (await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey) == null && ++time < command.FailoverCluster) ;
        }
    }

}
