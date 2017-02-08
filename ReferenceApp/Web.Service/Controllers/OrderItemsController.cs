// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Web.Service.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Common;
    using CustomerOrder.Domain;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Client;

    public class OrderItemsController : ApiController
    {
        private const string CustomerOrderServiceName = "CustomerOrderActorService";
        
        [HttpPost]
        [Route("api/orders/{orderId}/orderitems")]
        public async Task<Guid> PostItem(string orderId, CustomerOrderItem item)
        {
            ServiceEventSource.Current.Message("Guid {0}, quantity {1}", item.ItemId.ToString(), item.Quantity.ToString());

            Guid orderIdGuid = Guid.Parse(orderId);
            ServiceUriBuilder builder = new ServiceUriBuilder(CustomerOrderServiceName);

            //We create a unique Guid that is associated with a customer order, as well as with the actor that represents that order's state.
            ICustomerOrderActor customerOrder = ActorProxy.Create<ICustomerOrderActor>(new ActorId(orderIdGuid), builder.ToUri());

            try
            {
                await customerOrder.AddItemToOrderAsync(item);
                ServiceEventSource.Current.Message("Customer order submitted successfully. ActorOrderID: {0} created", orderId);
            }
            catch (InvalidOperationException ex)
            {
                ServiceEventSource.Current.Message("Web Service: Actor rejected {0}: {1}", customerOrder, ex);
                throw;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message("Web Service: Exception {0}: {1}", customerOrder, ex);
                throw;
            }

            return orderIdGuid;
        }
        
    }
}