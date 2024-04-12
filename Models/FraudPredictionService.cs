using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Intextwo.Models
{
    public class FraudPredictionService: IFraudPredictionService
    {
        private readonly InferenceSession _session;
        public FraudPredictionService(IWebHostEnvironment env)
        {
            // Build the path to the model file

            string modelPath;

            if (env.IsDevelopment())
            {
                // For local development, use the file directly from the project directory
                modelPath = Path.Combine(env.ContentRootPath, "fraud_pred (2).onnx");
            }
            else
            {
                // For production in Azure, use the file from the wwwroot directory
                // Adjust the path if your deployment process places the file elsewhere
                modelPath = Path.Combine(env.WebRootPath, "fraud_pred (2).onnx");
            }

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("The ONNX model file was not found.", modelPath);
            }

            // Create the inference session with the model
            _session = new InferenceSession(modelPath);
        }   



        public bool IsFraudulentOrder(Order order)
        {
            var inputs = PrepareInputTensor(order);


            var inputTensor = new DenseTensor<float>(inputs, new[] { 1, inputs.Length });
            var predictionInputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };
            try
            {
                using var results = _session.Run(predictionInputs);
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();

                // Assume fraud if prediction is 1, no fraud if 0
                return prediction != null && prediction.Length > 0 && prediction[0] == 1;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw new InvalidOperationException($"Error during fraud prediction: {ex.Message}", ex);
            }




        }








        private float[] PrepareInputTensor(Order order) //this function calls all the dummy coding function
        {
            var inputs = new float[49]; // Adjust the total number based on your actual model input

            SetDayOfWeekDummyCoding(order.day_of_week, inputs);
            SetTimeOfDayDummyCoding(order.time, inputs);
            SetEntryModeDummyCoding(order.entry_mode, inputs);
            SetTypeOfTransactionDummyCoding(order.type_of_transaction, inputs);
            SetCountryOfTransactionDummyCoding(order.country_of_transaction, inputs);
            SetShippingAddressDummyCoding(order.shipping_address, inputs);
            SetBankDummyCoding(order.bank, inputs);
            SetTypeOfCardDummyCoding(order.type_of_card, inputs);

            return inputs;
        }

        public void SetDayOfWeekDummyCoding(string dayOfWeek, float[] inputs) // This code adds the dummy coded day of the week to an array, with Friday dropped
        {
            var dayIndexMap = new Dictionary<string, int>
            {
                {"Mon", 0},
                {"Tue", 1},
                {"Wed", 2},
                {"Thu", 3},
                {"Sat", 4},
                {"Sun", 5}
            };
            // Reset the day of week fields to 0
            for (int i = 0; i < 6; i++) // There are now 6 active day indices because Friday is dropped
            {
                inputs[i] = 0;
            }

            if (dayOfWeek != null && dayIndexMap.TryGetValue(dayOfWeek, out int index))
            {
                inputs[index] = 1;
            }
        }

        public void SetTimeOfDayDummyCoding(int? time, float[] inputs)     //       24 options
        {
            // The time indices start at index 7, immediately after the day of week indices (which are 0 to 6)
            int baseIndex = 6;

            // Reset the time of day fields to 0
            for (int i = baseIndex; i < baseIndex + 24; i++) // There are 24 hours, from index 7 to 30
            {
                inputs[i] = 0;
            }

            // Set the appropriate index to 1 if time is valid (1 through 24)
            if (time.HasValue && time >= 1 && time <= 24)
            {
                inputs[baseIndex + time.Value - 1] = 1; // Subtract 1 because arrays are 0-indexed
            }
        } //this code adds the dummy codes for time of day field

        public void SetEntryModeDummyCoding(string entryMode, float[] inputs) //dummy codes entry mode     2 options
        {
            // Assuming the entry mode indices start at index 31
            int baseIndex = 30;

            // Reset the entry mode fields to 0
            inputs[baseIndex] = 0;   // Index for PIN
            inputs[baseIndex + 1] = 0;  // Index for Tap

            // Set the appropriate index to 1 based on the entry mode
            if (entryMode != null)
            {
                switch (entryMode)
                {
                    case "PIN":
                        inputs[baseIndex] = 1;
                        break;
                    case "Tap":
                        inputs[baseIndex + 1] = 1;
                        break;
                }
            }
        }

        public void SetTypeOfTransactionDummyCoding(string typeOfTransaction, float[] inputs) //dummy codes type of transaction      2 options
        {
            // Assuming the type of transaction indices start at index 33
            int baseIndex = 32;

            // Reset the type of transaction fields to 0
            inputs[baseIndex] = 0;   // Index for Online
            inputs[baseIndex + 1] = 0;  // Index for POS

            // Set the appropriate index to 1 based on the type of transaction
            if (typeOfTransaction != null)
            {
                switch (typeOfTransaction)
                {
                    case "Online":
                        inputs[baseIndex] = 1;
                        break;
                    case "POS":
                        inputs[baseIndex + 1] = 1;
                        break;
                }
            }
        }

        public void SetCountryOfTransactionDummyCoding(string countryOfTransaction, float[] inputs) //dummy codes country of transaction     4 options
        {
            // Define the starting index for country of transaction dummy coding
            int baseIndex = 35;

            // Reset the country of transaction fields to 0
            inputs[baseIndex] = 0;    // Index for India
            inputs[baseIndex + 1] = 0; // Index for Russia
            inputs[baseIndex + 2] = 0; // Index for USA
            inputs[baseIndex + 3] = 0; // Index for United Kingdom

            // Map countries to indices and set the appropriate index to 1
            if (!string.IsNullOrWhiteSpace(countryOfTransaction))
            {
                var countryIndexMap = new Dictionary<string, int>
                {
                    { "India", baseIndex },
                    { "Russia", baseIndex + 1 },
                    { "USA", baseIndex + 2 },
                    { "United Kingdom", baseIndex + 3 }
                };

                if (countryIndexMap.TryGetValue(countryOfTransaction, out int index))
                {
                    inputs[index] = 1;
                }
            }
        }

        public void SetShippingAddressDummyCoding(string shippingAddress, float[] inputs) // dummy codes shipping address     4 options
        {
            // Define the starting index for shipping address dummy coding
            int baseIndex = 39;

            // Reset the shipping address fields to 0
            inputs[baseIndex] = 0;    // Index for India
            inputs[baseIndex + 1] = 0; // Index for Russia
            inputs[baseIndex + 2] = 0; // Index for USA
            inputs[baseIndex + 3] = 0; // Index for United Kingdom

            // Map shipping address to indices and set the appropriate index to 1
            if (!string.IsNullOrWhiteSpace(shippingAddress))
            {
                var addressIndexMap = new Dictionary<string, int>
        {
            { "India", baseIndex },
            { "Russia", baseIndex + 1 },
            { "USA", baseIndex + 2 },
            { "United Kingdom", baseIndex + 3 }
        };

                if (addressIndexMap.TryGetValue(shippingAddress, out int index))
                {
                    inputs[index] = 1;
                }
            }
        }

        public void SetBankDummyCoding(string bank, float[] inputs)// dummy codes bank     6 dummy codes
        {
            // Define the starting index for bank dummy coding
            int baseIndex = 43;

            // Reset the bank fields to 0
            for (int i = baseIndex; i < baseIndex + 6; i++)
            {
                inputs[i] = 0;  // Indices for HSBC, Halifax, Lloyds, Metro, Monzo, RBS
            }

            // Map banks to indices and set the appropriate index to 1
            if (!string.IsNullOrWhiteSpace(bank))
            {
                var bankIndexMap = new Dictionary<string, int>
                {
                    { "HSBC", baseIndex },
                    { "Halifax", baseIndex + 1 },
                    { "Lloyds", baseIndex + 2 },
                    { "Metro", baseIndex + 3 },
                    { "Monzo", baseIndex + 4 },
                    { "RBS", baseIndex + 5 }
                };

                if (bankIndexMap.TryGetValue(bank, out int index))
                {
                    inputs[index] = 1;
                }
            }
        }

        public void SetTypeOfCardDummyCoding(string typeOfCard, float[] inputs) // dummy code for card type   1 dummy code
        {
            int visaIndex = 48; // Index for Visa

            // Reset the type of card field to 0
            inputs[visaIndex] = 0; // Only one index since we're using drop-first

            // Set the index to 1 if the type of card is Visa
            if (typeOfCard != null && typeOfCard == "Visa")
            {
                inputs[visaIndex] = 1;
            }
        }

    }
}
