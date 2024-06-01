using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AG.Dns.Requests.Models;

namespace AG.Dns.Requests.Services
{
    // Units class for unit conversions.
    public class UnitConverter : IConverter
    {
        private readonly Dictionary<string, Dictionary<string, Unit>> units = new();
        private readonly Dictionary<string, AG.Dns.Requests.Models.Group> symbols = new();
        private readonly List<string> help = new();

        private static readonly Regex reParse = new(@"(?i)([0-9\.]+)([a-z]{1,6})\-([a-z]{1,6})", RegexOptions.Compiled);

        // Load units from embedded JSON file.
        public UnitConverter()
        {
            LoadUnits();
            help = PrintUnitsList();
        }

        // Parse and execute a unit conversion query.
        public List<string> Query(string query)
        {
            if (query == "unit.")
            {
                return help;
            }

            var match = reParse.Match(query);
            if (!match.Success || match.Groups.Count != 4)
            {
                throw new ArgumentException("Invalid unit query.");
            }

            // Parse the numeric value.
            if (!double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                throw new ArgumentException("Invalid number.");
            }

            var fromSymbol = match.Groups[2].Value.ToLowerInvariant();
            var toSymbol = match.Groups[3].Value.ToLowerInvariant();

            // Validate unit symbols.
            if (!symbols.TryGetValue(fromSymbol, out var fromGroup))
            {
                throw new ArgumentException($"Unknown unit: {fromSymbol}. Use 'unit.' to see the list of units.");
            }

            if (!units[fromGroup.Name].TryGetValue(fromSymbol, out var fromUnit))
            {
                throw new ArgumentException($"Unknown unit: {fromSymbol}. Use 'unit.' to see the list of units.");
            }

            if (!symbols.TryGetValue(toSymbol, out var toGroup))
            {
                throw new ArgumentException($"Unknown unit: {toSymbol}. Use 'unit.' to see the list of units.");
            }

            if (!units[toGroup.Name].TryGetValue(toSymbol, out var toUnit))
            {
                throw new ArgumentException($"Unknown unit: {toSymbol}. Use 'unit.' to see the list of units.");
            }

            // Ensure both units are from the same group for conversion.
            if (fromGroup.Name != toGroup.Name)
            {
                throw new ArgumentException($"Cannot convert {fromSymbol} ({fromUnit.Name}) to {toSymbol} ({toUnit.Name}).");
            }

            var baseRate = units[fromGroup.Name][fromGroup.BaseSymbol].Value;

            // Perform the conversion.
            var conversionValue = (baseRate / fromUnit.Value) / (baseRate / toUnit.Value) * value;
            //var result = $"{query} 1 TXT \"{value:0.00} {fromUnit.Name} ({fromUnit.Symbol}) = {conversionValue:0.00} {toUnit.Name} ({toUnit.Symbol})\"";

            var result = $"{value:0.00} {fromUnit.Name} ({fromUnit.Symbol}) = {conversionValue:0.00} {toUnit.Name} ({toUnit.Symbol})";
            //var result = $"{value:0.00} {fromUnit.Name} ({fromUnit.Symbol}) = {conversionValue:0.00} {toUnit.Name} ({toUnit.Symbol})";
            //var result = $"{conversionValue:0.00} ({toUnit.Symbol})";
            //var result = "";
            return new List<string> { result };
        }

        // Print the list of available units.
        private List<string> PrintUnitsList()
        {
            var output = new List<string>();
            var groupNames = units.Keys.OrderBy(name => name).ToList();

            foreach (var groupName in groupNames)
            {
                var unitList = units[groupName].Values.OrderBy(u => u.Symbol).ToList();
                foreach (var unit in unitList)
                {
                    output.Add($"unit. 1 TXT \"{groupName}\" \"{unit.Symbol} ({unit.Name})\"");
                }
            }

            return output;
        }

        // Load units from a JSON file.
        private void LoadUnits()
        {
            var json = File.ReadAllText(@"configs\units.json");
            var data = JsonSerializer.Deserialize<Dictionary<string, FileData>>(json);

            foreach (var kvp in data)
            {
                var groupName = kvp.Key;
                var fileData = kvp.Value;

                units[groupName] = new Dictionary<string, Unit>();
                foreach (var unit in fileData.Units)
                {
                    symbols[unit.Symbol.ToLowerInvariant()] = new AG.Dns.Requests.Models.Group { Name = groupName, BaseSymbol = fileData.BaseSymbol };
                    units[groupName][unit.Symbol.ToLowerInvariant()] = unit;
                }
            }
        }
    }
}

