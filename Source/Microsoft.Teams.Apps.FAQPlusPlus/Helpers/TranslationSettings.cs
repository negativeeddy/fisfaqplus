namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// General translation settings and constants.
    /// </summary>
    public class TranslationSettings
    {
        public string DefaultLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationSettings"/> class.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public TranslationSettings(IConfiguration configuration)
        {
            var defaultLanguage = configuration["DefaultLanguage"];
            this.DefaultLanguage = defaultLanguage ?? throw new ArgumentNullException(nameof(defaultLanguage));
        }

    }
}
