/*
* DRBDBReader
* Copyright (C) 2026 Ivan Feoctistov
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;
using DRBDBReader.DB;
using DRBDBReader.DB.Records;

namespace DRBDBReader
{
	public class CLI
	{
		private enum OpStatus { OK, Warning, Error };
		private Database _db;
		public void Run()
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			Console.InputEncoding = System.Text.Encoding.UTF8;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Write("Loading database.mem... ");
			try
			{
				_db = new Database(new FileInfo("database.mem"));
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("done!");
				Console.ResetColor();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("error!");
				Console.WriteLine($"{e.Message}");
				Console.ResetColor();
				return;
			}
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Enter command below and press \"enter\".");
			Console.WriteLine("Command \"help\" will show the list of all available commands.");
			Console.ResetColor();

			while (true)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.Write(">");
				Console.ResetColor();

				string input = Console.ReadLine()?.Trim();
				if (string.IsNullOrEmpty(input)) continue;
				if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;
				var (status, result) = ProcessCommand(input);
				Console.ForegroundColor = status switch
				{
					OpStatus.Warning => ConsoleColor.Yellow,
					OpStatus.Error => ConsoleColor.Red,
					_ => ConsoleColor.White,
				};
				Console.WriteLine(result);
				Console.ResetColor();
				Console.WriteLine();
			}
		}

		private (OpStatus, string) ProcessCommand(string input)
		{
			try
			{
				input = input.ToLowerInvariant();
				string[] insplit = input.Split(' ', 2);
				string command = insplit[0];
				bool argPresent = insplit.Length > 1 && !string.IsNullOrWhiteSpace(insplit[1]);
				string arg = argPresent ? insplit[1] : string.Empty;

				return command switch
				{
					"help" => (OpStatus.OK, GetHelp()),

					"stringid" => argPresent ? ExecuteStringID(arg) : (OpStatus.Warning, "define ID to search"),
					"stringsearch" => argPresent ? ExecuteStringSearch(arg) : (OpStatus.Warning, "define text to search"),

					"txid" => argPresent ? ExecuteTxID(arg) : (OpStatus.Warning, "define TX ID"),
					"txrunconverter" => argPresent ? ExecuteTxConverter(arg, imperial: true) : (OpStatus.Warning, "define data???"),
					"txrunconvertermetric" => argPresent ? ExecuteTxConverter(arg, imperial: false) : (OpStatus.Warning, "define data???"),
					"txsearch" => argPresent ? ExecuteTxSearch(arg) : (OpStatus.Warning, "define text to search"),
					"dumpstateconverter" or "dumpconverter" or "convertertostring" => argPresent ? ExecuteDumpConverter(arg) : (OpStatus.Warning, "define TX ID"),

					"modid" => argPresent ? ExecuteModID(arg) : (OpStatus.Warning, "define module ID"),
					"modlist" => ExecuteModSearch(string.Empty, isListRequest: true),
					"modsearch" => argPresent ? ExecuteModSearch(arg, isListRequest: false) : (OpStatus.Warning, "define text"),
					"modtxlist" => argPresent ? ExecuteModTxList(arg) : (OpStatus.Warning, "define mod ID"),

					"dumptableinfo" => argPresent ? ExecuteDumpTableInfo(arg) : (OpStatus.Warning, "define data???"),
					"stringidfuzz" => argPresent ? ExecuteStringIDFuzz(arg) : (OpStatus.Warning, "define data??? 2 params?"),
					"genericidfuzz" => argPresent ? ExecuteGenericIDFuzz(arg) : (OpStatus.Warning, "define data??? 4 params?"),

					_ => (OpStatus.Error, $"unknown command '{command}'. Use 'help' to list all supported commands.")
				};
			}
			catch (Exception e)
			{
				return (OpStatus.Error, $"{e.Message}");
			}
		}

		private string GetHelp()
		{
			return "available commands:" + Environment.NewLine +
			"\tstringid [id] : find string by ID" + Environment.NewLine +
			"\tstringsearch [text] : find string by content" + Environment.NewLine +
			"\ttxid [id] : get details of TX message" + Environment.NewLine +
			"\ttasearch [text] : search for TX messages by content" + Environment.NewLine +
			"\ttxrunconverter [id] [data] : conv???" + Environment.NewLine +
			"\ttxrunconvertermetric [id] [data] : conv???" + Environment.NewLine +
			"\tdumpconverter [id] : ???" + Environment.NewLine +
			"\tmodid [id] : ???" + Environment.NewLine +
			"\tmodlist : ???" + Environment.NewLine +
			"\tmodsearch [text] : ???" + Environment.NewLine +
			"\tmodtxlist [id] : ???" + Environment.NewLine +
			"\tdumptableinfo [id] : ???" + Environment.NewLine +
			"\tstringidfuzz [id] [col] : ???" + Environment.NewLine +
			"\tgenericidfuzz [id] [col] [id2] [col2] : ???" + Environment.NewLine +
			"\texit : ???";
		}

		private static ushort ParseU16(string input)
		{
			ushort result;

			if (input.StartsWith("0x"))
			{
				result = Convert.ToUInt16(input[2..], 16);
			}
			else if (input.StartsWith("0b"))
			{
				result = Convert.ToUInt16(input[2..], 2);
			}
			else if (input.StartsWith("0o"))
			{
				result = Convert.ToUInt16(input[2..], 8);
			}
			else
			{
				result = Convert.ToUInt16(input);
			}

			return result;
		}

		private static uint ParseU32(string input)
		{
			uint result;

			if (input.StartsWith("0x"))
			{
				result = Convert.ToUInt32(input[2..], 16);
			}
			else if (input.StartsWith("0b"))
			{
				result = Convert.ToUInt32(input[2..], 2);
			}
			else if (input.StartsWith("0o"))
			{
				result = Convert.ToUInt32(input[2..], 8);
			}
			else
			{
				result = Convert.ToUInt32(input);
			}

			return result;
		}

		private static long ParseI64(string input)
		{
			long result;

			if (input.StartsWith("0x"))
			{
				result = Convert.ToInt64(input[2..], 16);
			}
			else if (input.StartsWith("0b"))
			{
				result = Convert.ToInt64(input[2..], 2);
			}
			else if (input.StartsWith("0o"))
			{
				result = Convert.ToInt64(input[2..], 8);
			}
			else
			{
				result = Convert.ToInt64(input);
			}

			return result;
		}

		private (OpStatus, string) ExecuteStringID(string arg)
		{
			ushort id = ParseU16(arg);
			var record = (StringRecord)_db.tables[Database.TABLE_STRINGS].getRecord(id);

			if (record == null) return (OpStatus.OK, "(null)");
			string result = $"text: {record.text}";
			if (!string.IsNullOrWhiteSpace(record.obdCodeString)) result += $"; OBD: {record.obdCodeString}";

			return (OpStatus.OK, result);
		}

		private (OpStatus, string) ExecuteStringSearch(string arg)
		{
			var s = new StringBuilder();

			foreach (StringRecord r in _db.tables[Database.TABLE_STRINGS].records)
			{
				bool matchText = r.text?.ToLowerInvariant().Contains(arg) ?? false;
				bool matchOBD = r.obdCodeString?.ToLowerInvariant().Contains(arg) ?? false;
				if (matchText || matchOBD)
				{
					s.Append($"0x{r.id:x4}; text: {r.text}");
					if (!string.IsNullOrWhiteSpace(r.obdCodeString)) s.Append($"; OBD: {r.obdCodeString}");
					s.AppendLine();
				}
			}

			return s.Length > 0 ? (OpStatus.OK, s.ToString().TrimEnd()) : (OpStatus.Warning, "not found");
		}

		private (OpStatus, string) ExecuteTxID(string arg)
		{
			uint id = ParseU32(arg);
			return (OpStatus.OK, _db.getDetailedTX(id));
		}

		private (OpStatus, string) ExecuteTxConverter(string arg, bool imperial)
		{
			string[] argsplit = arg.Split(' ', 2);
			if (argsplit.Length < 2) return (OpStatus.Warning, "this command needs 2 params: [txid] [data]");
			uint id = ParseU32(argsplit[0]);
			long data = ParseI64(argsplit[1]);
			TXRecord record = (TXRecord)_db.tables[Database.TABLE_TRANSMIT].getRecord(id);

			if (record?.converter == null) return (OpStatus.Warning, "converter not found");
			string result = record.converter.processData(data, outputMetric: !imperial);

			return (OpStatus.OK, result);
		}

		private (OpStatus, string) ExecuteTxSearch(string arg)
		{
			var s = new StringBuilder();
			string[] args = arg.Contains(" && ") ? arg.Split(" && ", StringSplitOptions.RemoveEmptyEntries) : null;

			for (uint i = 0x80000000; i < 0x80009000; i++)
			{
				try
				{
					string tmp = _db.getTX(i).ToLowerInvariant();
					if (tmp == null) continue;
					if (args != null)
					{
						bool match = true;
						foreach (string f in args)
						{
							if (!tmp.Contains(f))
							{
								match = false;
								break;
							}
							if (match) s.AppendLine($"{tmp}; 0x{i:x}");
						}
					}
					else if (tmp.Contains(arg))
					{
						s.AppendLine($"{tmp}; 0x{i:x}");
					}
				}
				catch
				{
					//ignore
				}
			}

			return s.Length > 0 ? (OpStatus.OK, s.ToString().TrimEnd()) : (OpStatus.Warning, "not found");
		}

		private (OpStatus, string) ExecuteDumpConverter(string arg)
		{
			uint id = ParseU32(arg);
			TXRecord record = (TXRecord)_db.tables[Database.TABLE_TRANSMIT].getRecord(id);

			if (record?.converter == null) return (OpStatus.Warning, "converter not found");

			return (OpStatus.OK, record.converter.ToString());
		}

		private (OpStatus, string) ExecuteModID(string arg)
		{
			ushort id = ParseU16(arg);
			string result = _db.getModule(id);
			return result != null ? (OpStatus.OK, result) : (OpStatus.Warning, "not found");
		}

		private (OpStatus, string) ExecuteModSearch(string arg, bool isListRequest)
		{
			var s = new StringBuilder();
			string[] args = arg.Contains(" && ") ? arg.Split(" && ", StringSplitOptions.RemoveEmptyEntries) : null;

			for (ushort i = 0x0000; i < 0x2000; i++)
			{
				try
				{
					string tmp = _db.getModule(i);
					if (tmp == null) continue;
					if (isListRequest)
					{
						s.AppendLine($"{tmp}; 0x{i:x}");
						continue;
					}

					string tmpl = tmp.ToLowerInvariant(); //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
					if (args != null)
					{
						bool match = true;
						foreach (string f in args)
						{
							if (!tmpl.Contains(f)) //@@@@@@@@@@@ MAYBE NOT NEEDED AND CAN BE COMPARED WITH CASE INSENSITIVE FUNCTION INSTEAD OF MULTIME CONVERSIONS @@@@@@@@@@@@@@@@@@@@@
							{
								match = false;
								break;
							}
						}
						if (match) s.AppendLine($"{tmp}; 0x{i:x}"); ////@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
					}
					else if (tmpl.Contains(arg))
					{
						s.AppendLine($"{tmp}; 0x{i:x}");
					}
				}
				catch
				{
					//ignore
				}
			}

			return s.Length > 0 ? (OpStatus.OK, s.ToString().TrimEnd()) : (OpStatus.Warning, "not found");
		}

		private (OpStatus, string) ExecuteModTxList(string arg)
		{
			ushort id = ParseU16(arg);
			Record record = _db.tables[Database.TABLE_MODULE].getRecord(id);

			if (record == null) return (OpStatus.Warning, "not found");

			var s = new StringBuilder();
			var mrecord = (ModuleRecord)record;

			foreach (TXRecord txrec in mrecord.dataelements)
			{
				string tmp = _db.getTX(txrec.id);
				s.AppendLine($"{tmp}; 0x{txrec.id:x}");
			}

			return s.Length > 0 ? (OpStatus.OK, s.ToString().TrimEnd()) : (OpStatus.Warning, "not found");
		}

		private (OpStatus, string) ExecuteDumpTableInfo(string arg)
		{
			ushort id = ParseU16(arg);
			Table t = _db.tables[id];
			string result = $"Table: {id}; Columns: {t.colCount}; Rows: {t.rowCount}" + Environment.NewLine +
							$"ColSizes: {BitConverter.ToString(t.colSizes)}; RowSize: {t.rowSize};";
			return (OpStatus.OK, result);
		}

		private (OpStatus, string) ExecuteStringIDFuzz(string arg)
		{
			string[] argsplit = arg.Split(' ', 2);
			if (argsplit.Length < 2) return (OpStatus.Warning, "not enough data, need [table] [column]");
			ushort id = ParseU16(argsplit[0]);
			byte column = (byte)ParseU16(argsplit[1]);
			Table t = _db.tables[id];
			int hits = 0;
			int zeros = 0;

			foreach (Record r in t.records)
			{
				ushort field = (ushort)t.readField(r, column);
				string str = _db.getString(field);
				if (str != "(null)") hits++;
				if (field == 0) zeros++;
			}

			return (OpStatus.OK, $"Records: {t.records.Length}; Hits: {hits}; Zeroes: {zeros};");
		}

		private (OpStatus, string) ExecuteGenericIDFuzz(string arg)
		{
			string[] argsplit = arg.Split(' ', 2);
			if (argsplit.Length < 4) return (OpStatus.Warning, "not enough data, need [fuzzertable] [fuzzercolumn] [fuzzingtable] [fuzzingcolumn]");
			ushort fuzzerTableID = ParseU16(argsplit[0]);
			byte fuzzerTableColumn = (byte)ParseU16(argsplit[1]);
			ushort fuzzingTableID = ParseU16(argsplit[2]);
			byte fuzzingTableColumn = (byte)ParseU16(argsplit[3]);
			Table fuzzerTable = _db.tables[fuzzerTableID];
			Table fuzzingTable = _db.tables[fuzzingTableID];
			int hits = 0;
			int zeros = 0;

			foreach (Record fuzzingRecord in fuzzingTable.records)
			{
				uint fuzzingRecordID = (uint)fuzzingTable.readField(fuzzingRecord, fuzzingTableColumn);
				Record fuzzerRecord = fuzzerTable.getRecord(fuzzingRecordID, idcol: fuzzerTableColumn, sorted: false);
				if (fuzzerRecord != null) hits++;
				if (fuzzingRecordID == 0) zeros++;
			}

			return (OpStatus.OK, $"Records: {fuzzingTable.records.Length}; Hits: {hits}; Zeroes: {zeros};");
		}
	}
}
