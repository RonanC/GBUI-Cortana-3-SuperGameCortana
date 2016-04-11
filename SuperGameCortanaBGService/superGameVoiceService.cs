using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace SuperGameCortanaBGService
{
    public sealed class superGameVoiceService : IBackgroundTask
    {
        private BackgroundTaskDeferral _defferal;
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // inform the system that the background task may continue ater the run method has completed
            this._defferal = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if ((triggerDetails != null) && (triggerDetails.Name == "superGameVoiceService"))
            {
                try
                {
                    voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    voiceServiceConnection.VoiceCommandCompleted += VoiceServiceConnection_VoiceCommandCompleted;
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    switch (voiceCommand.CommandName)
                    {
                        case "highScorer":
                            {
                                sendCompletionMessageForHighScorer();
                                break;
                            }
                        default:
                            {
                                launchAppInForeground();
                                break;
                            }

                    }
                }
                finally
                {
                    if (this._defferal != null)
                    {
                        // complete the deferral
                        this._defferal.Complete();
                    }
                }
            } // end if (trigger details)
        }

        private async void launchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Launching superGame";

            var response = VoiceCommandResponse.CreateResponse(userMessage);
            response.AppLaunchArgument = "";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private async void sendCompletionMessageForHighScorer()
        {
            // longer than 0.5 seconds, then progress report has to be sent
            string progressMessage = "Finding the highest scorer";
            await ShowProgressScreen(progressMessage);

            var userMsg = new VoiceCommandUserMessage();
            userMsg.DisplayMessage = userMsg.SpokenMessage = "The person with the highest score is Ronan.";

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMsg);
            await voiceServiceConnection.ReportSuccessAsync(response);
        }

        private async Task ShowProgressScreen(string progressMessage)
        {
            var userProgressMsg = new VoiceCommandUserMessage();
            userProgressMsg.DisplayMessage = userProgressMsg.SpokenMessage = progressMessage;
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMsg);
            await voiceServiceConnection.ReportProgressAsync(response);
        }

        private void VoiceServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this._defferal != null)
            {
                this._defferal.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this._defferal != null)
            {
                this._defferal.Complete();
            }
        }
    }
}
