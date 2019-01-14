﻿// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class ScriptGenerator
    {
        private readonly IConsole _console;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScriptGenerator> _logger;

        public ScriptGenerator(IConsole console, IServiceProvider serviceProvider)
        {
            _console = console;
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<ScriptGenerator>>();
        }

        public bool TryGenerateScript(out string generatedScript)
        {
            generatedScript = null;

            try
            {
                var options = _serviceProvider.GetRequiredService<IOptions<BuildScriptGeneratorOptions>>().Value;
                var scriptGenerator = _serviceProvider.GetRequiredService<IScriptGenerator>();
                var sourceRepoProvider = _serviceProvider.GetRequiredService<ISourceRepoProvider>();

                var sourceRepo = sourceRepoProvider.GetSourceRepo();
                var scriptGeneratorContext = new ScriptGeneratorContext
                {
                    SourceRepo = sourceRepo,
                    Language = options.Language,
                    LanguageVersion = options.LanguageVersion,
                    DestinationDir = options.DestinationDir,
                    Properties = options.Properties
                };

                // Try generating a script
                if (!scriptGenerator.TryGenerateBashScript(scriptGeneratorContext, out generatedScript))
                {
                    _console.Error.WriteLine(
                        "Error: Could not find a script generator which can generate a script for " +
                        $"the code in '{options.SourceDir}'.");
                    return false;
                }

                // Replace any CRLF with LF
                generatedScript = generatedScript.Replace("\r\n", "\n");

                return true;
            }
            catch (InvalidUsageException ex)
            {
                _logger.LogError(ex, "Invalid usage");
                _console.Error.WriteLine(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception caught");
                _console.Error.WriteLine(Constants.GenericErrorMessage);
                return false;
            }
        }
    }
}
