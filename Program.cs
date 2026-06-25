/*
 * DRBDBReader
 * Copyright (C) 2016, Kyle Repinski
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

namespace DRBDBReader
{
	internal sealed class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		private static void Main()
		{
			CLI terminal = new();
			terminal.Run();
		}
	}
}
