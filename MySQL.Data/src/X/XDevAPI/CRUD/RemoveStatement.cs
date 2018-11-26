// Copyright (c) 2015, 2018, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using MySqlX.XDevAPI.Common;


namespace MySqlX.XDevAPI.CRUD
{
  /// <summary>
  /// Represents a chaining collection remove statement.
  /// </summary>
  public class RemoveStatement : FilterableStatement<RemoveStatement, Collection, Result>
  {
    internal RemoveStatement(Collection collection, string condition) : base(collection, condition)
    {
    }

    /// <summary>
    /// Sets user-defined sorting criteria for the operation. The strings use normal SQL syntax like
    /// "order ASC" or "pages DESC, age ASC".
    /// </summary>
    /// <param name="order">The order criteria.</param>
    /// <returns>A generic object representing the implementing statement type.</returns>
    public RemoveStatement Sort(params string[] order)
    {
      FilterData.OrderBy = order;
      return this;
    }

    /// <summary>
    /// Executes the remove statement.
    /// </summary>
    /// <returns>A <see cref="Result"/>object containing the results of the execution.</returns>
    public override Result Execute()
    {
      return Execute(Target.Session.XSession.DeleteDocs, this);
    }
  }
}