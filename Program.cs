// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;

namespace ManagePolicyDefinition
{
    public class Program
    {
        /**
        * Azure PolicyDefinition sample for managing policy definitions -
        * - Create a policy definition
        * - Create another policy definition
        * - List policy definitions
        * - Delete policy definitions.
        */
        public static async Task RunSample(ArmClient client)
        {
            var policyDefinitionName1 = "pd1";
            var policyDefinitionName2 = "pd2";
            var indexedMode = "Indexed";
            var policyRuleJson = "{\"if\":{\"not\":{\"field\":\"location\",\"in\":[\"northeurope\",\"westeurope\"]}},\"then\":{\"effect\":\"deny\"}}";

            var subscription = await client.GetDefaultSubscriptionAsync();

            try
            {
                //=============================================================
                // Create policy definition.

                Console.WriteLine($"Creating a policy definition with name: {policyDefinitionName1}");

                var policyDefinitionData = new PolicyDefinitionData
                {
                    PolicyRule = BinaryData.FromString(policyRuleJson),
                    PolicyType = PolicyType.Custom,
                };
                // this operation returns an ArmOperation which is used to track the status of an operation that might take a long time
                // by passing `WaitUntil.Completed` will automatically make the function to wait for the completion of this operation
                var lro = await subscription.GetSubscriptionPolicyDefinitions().CreateOrUpdateAsync(WaitUntil.Completed, policyDefinitionName1, policyDefinitionData);
                var policyDefinition = lro.Value;

                Console.WriteLine($"Policy definition created: {policyDefinition.Id}");

                //=============================================================
                // Create another policy definition.

                Console.WriteLine($"Creating another policy definition with name: {policyDefinitionName2}");

                policyDefinitionData = new PolicyDefinitionData
                {
                    PolicyRule = BinaryData.FromString(policyRuleJson),
                    PolicyType = PolicyType.Custom,
                    Mode = indexedMode,
                };
                lro = await subscription.GetSubscriptionPolicyDefinitions().CreateOrUpdateAsync(WaitUntil.Completed, policyDefinitionName2, policyDefinitionData);
                var policyDefinition2 = lro.Value;

                Console.WriteLine($"Policy definition created: {policyDefinition2.Id}");

                //=============================================================
                // List policy definitions.

                Console.WriteLine("Listing all policy definitions: ");

                await foreach (var pDefinition in subscription.GetSubscriptionPolicyDefinitions())
                {
                    Console.WriteLine($"Policy definition: {pDefinition.Id}");
                }

                //=============================================================
                // Delete policy definitions.

                Console.WriteLine($"Deleting policy definition: {policyDefinitionName1}");

                await policyDefinition.DeleteAsync(WaitUntil.Completed);

                Console.WriteLine($"Deleted policy definition: {policyDefinitionName1}");

                Console.WriteLine($"Deleting policy definition: {policyDefinitionName2}");

                await policyDefinition2.DeleteAsync(WaitUntil.Completed);

                Console.WriteLine($"Deleted policy definition: {policyDefinitionName2}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var credential = new DefaultAzureCredential();

                var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
                // you can also use `new ArmClient(credential)` here, and the default subscription will be the first subscription in your list of subscription
                var client = new ArmClient(credential, subscriptionId);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
