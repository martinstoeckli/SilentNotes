// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Globalization;

namespace SilentNotes.Crypto.KeyDerivation
{
    /// <summary>
    /// The cost parameters for the Argon2 key derivation function.
    /// </summary>
    public class Argon2Cost
    {
        /// <summary>
        /// Tries to parse the formatted cost parameter string.
        /// </summary>
        /// <param name="cost">Formatted cost parameter string, created by the <see cref="Format"/>
        /// function. Example: "m=1024,t=3,p=2"</param>
        /// <param name="costParameters">Retrieves the cost parameters in case of success.</param>
        /// <returns>Returns true if the cost parameter string could be successfully read,
        /// otherwise false.</returns>
        public static bool TryParse(string cost, out Argon2Cost costParameters)
        {
            costParameters = null;
            try
            {
                string[] costParts = cost.Split(',', StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, string> costDictionary = costParts.Select(
                    costPart => costPart.Split('=')).ToDictionary(s => s[0], s => s[1]);

                int memory = int.Parse(costDictionary["m"], NumberStyles.None);
                int iterations = int.Parse(costDictionary["t"], NumberStyles.None);
                int parallelism = int.Parse(costDictionary["p"], NumberStyles.None);

                costParameters = new Argon2Cost
                {
                    MemoryKib = memory,
                    Iterations = iterations,
                    Parallelism = parallelism
                };
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats the cost parameters to a string which can be stored together with the key.
        /// </summary>
        /// <returns>Formatted string containing the cost parameters.</returns>
        public string Format()
        {
            return string.Format("m={0},t={1},p={2}", MemoryKib, Iterations, Parallelism);
        }

        /// <summary>
        /// Gets or sets the amount of memory in KiB used to do the Argon2 calculation.
        /// </summary>
        public int MemoryKib { get; set; }

        /// <summary>
        /// Gets or sets the number of iterations done to do the Argon2 calculation.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// Gets or sets the number of parallel threads used to do the Argon2 calculation.
        /// </summary>
        public int Parallelism { get; set; }
    }
}
