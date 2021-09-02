using System;
using EventHubService.Models;

namespace EventHubService.Services.Validators
{
    public class RootValidator : IValidate<Root>
    {
        public bool Validate(Root receivedModel)
        {
            if (receivedModel == null)
                return false;

            if (receivedModel.Id.Length <= 1 || receivedModel.Id == null)
                return false;

            if (receivedModel.UserId.Length <= 1 || receivedModel.UserId == null)
                return false;

            DateTime dateTime;
            if (!DateTime.TryParse(receivedModel.Timestamp, out dateTime) || receivedModel.Timestamp == null)
                return false;

            return true;
        }
    }
}