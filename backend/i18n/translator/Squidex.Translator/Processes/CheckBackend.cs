﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Translator.State;

namespace Squidex.Translator.Processes;

public class CheckBackend(DirectoryInfo folder, TranslationService service)
{
    private readonly DirectoryInfo folder = Backend.GetFolder(folder);

    public void Run()
    {
        var all = new HashSet<string>();

        foreach (var (file, relativeName) in Backend.GetFiles(folder))
        {
            var content = File.ReadAllText(file.FullName);

            var translations = new HashSet<string>();

            void AddTranslations(string regex)
            {
                var matches = Regex.Matches(content, regex, RegexOptions.Singleline | RegexOptions.ExplicitCapture);

                foreach (Match match in matches)
                {
                    var key = match.Groups["Key"].Value;

                    translations.Add(key);

                    all.Add(key);
                }
            }

            AddTranslations("T\\.Get\\(\"(?<Key>[^\"]*)\"");
            AddTranslations("\"(?<Key>history\\.[^\"]*)\"");

            Helper.CheckForFile(service, relativeName, translations);
        }

        Helper.CheckUnused(service, all);
        Helper.CheckOtherLocales(service);

        service.Save();
    }
}
