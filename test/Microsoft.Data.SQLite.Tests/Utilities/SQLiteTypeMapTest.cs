// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Xunit;

namespace Microsoft.Data.SQLite.Utilities
{
    public class SQLiteTypeMapTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData((byte)1)]
        [InlineData((sbyte)1)]
        [InlineData(1)]
        [InlineData(1u)]
        [InlineData(1L)]
        [InlineData(1ul)]
        [InlineData((short)1)]
        [InlineData((ushort)1)]
        public void FromClrType_maps_integers(object value)
        {
            var map = SQLiteTypeMap.FromClrType(value.GetType());

            Assert.Equal(value.GetType(), map.ClrType);
            Assert.Equal(SQLiteType.Integer, map.SQLiteType);
            Assert.Equal(1L, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(1L));
        }

        [Theory]
        [InlineData(3.14)]
        [InlineData(3.14f)]
        public void FromClrType_maps_floats(object value)
        {
            var map = SQLiteTypeMap.FromClrType(value.GetType());

            Assert.Equal(SQLiteType.Float, map.SQLiteType);
            Assert.Equal(3.14, (double)map.ToInterop(value), precision: 6);
            Assert.Equal(value, map.FromInterop(3.14));
        }

        [Fact]
        public void FromClrType_maps_string_to_text()
        {
            var value = "test";

            var map = SQLiteTypeMap.FromClrType<string>();

            Assert.Equal(typeof(string), map.ClrType);
            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("test", map.ToInterop(value));
            Assert.Equal(value, map.FromInterop("test"));
        }

        [Fact]
        public void FromClrType_maps_byteArray_to_blob()
        {
            var value = new byte[] { 0x7e, 0x57 };

            var map = SQLiteTypeMap.FromClrType<byte[]>();

            Assert.Equal(typeof(byte[]), map.ClrType);
            Assert.Equal(SQLiteType.Blob, map.SQLiteType);
            Assert.Equal(new byte[] { 0x7e, 0x57 }, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(new byte[] { 0x7e, 0x57 }));
        }

        [Fact]
        public void FromClrType_maps_dbNull_to_null()
        {
            var value = DBNull.Value;

            var map = SQLiteTypeMap.FromClrType<DBNull>();

            Assert.Equal(typeof(DBNull), map.ClrType);
            Assert.Equal(SQLiteType.Null, map.SQLiteType);
            Assert.Equal(DBNull.Value, map.ToInterop(value));
            Assert.Equal(value, map.FromInterop(DBNull.Value));
        }

        [Fact]
        public void FromClrType_throws_when_unknown()
        {
            var ex = Assert.Throws<ArgumentException>(() => SQLiteTypeMap.FromClrType(GetType()));
            Assert.Equal(Strings.FormatUnknownDataType(GetType()), ex.Message);
        }

        [Fact]
        public void FromClrType_handles_nullable_types()
        {
            Assert.Equal(typeof(int), SQLiteTypeMap.FromClrType(typeof(int?)).ClrType);
        }

        [Fact]
        public void FromClrType_handles_enum_types()
        {
            Assert.Equal(typeof(int), SQLiteTypeMap.FromClrType(typeof(StringComparison)).ClrType);
        }

        [Theory]
        [InlineData("BIT", SQLiteType.Integer, typeof(bool), DbType.Boolean)]
        [InlineData("BLOB", SQLiteType.Blob, typeof(byte[]), DbType.Binary)]
        [InlineData("CHAR", SQLiteType.Text, typeof(string), DbType.String)]
        [InlineData("DATETIME", SQLiteType.Text, typeof(DateTime), DbType.DateTime)]
        [InlineData("DATETIMEOFFSET", SQLiteType.Text, typeof(DateTimeOffset), DbType.DateTimeOffset)]
        [InlineData("DECIMAL", SQLiteType.Text, typeof(decimal), DbType.Decimal)]
        [InlineData("FLOAT", SQLiteType.Float, typeof(double), DbType.Double)]
        [InlineData("INT", SQLiteType.Integer, typeof(int), DbType.Int32)]
        [InlineData("INT8", SQLiteType.Integer, typeof(sbyte), DbType.SByte)]
        [InlineData("INTEGER", SQLiteType.Integer, typeof(long), DbType.Int64)]
        [InlineData("INTERVAL", SQLiteType.Text, typeof(TimeSpan), DbType.Time)]
        [InlineData("NCHAR", SQLiteType.Text, typeof(string), DbType.String)]
        [InlineData("NVARCHAR", SQLiteType.Text, typeof(string), DbType.String)]
        [InlineData("REAL", SQLiteType.Float, typeof(double), DbType.Double)]
        [InlineData("SINGLE", SQLiteType.Float, typeof(float), DbType.Single)]
        [InlineData("SMALLINT", SQLiteType.Integer, typeof(short), DbType.Int16)]
        [InlineData("TINYINT", SQLiteType.Integer, typeof(byte), DbType.Byte)]
        [InlineData("UINT", SQLiteType.Integer, typeof(uint), DbType.UInt32)]
        [InlineData("UINT16", SQLiteType.Integer, typeof(ushort), DbType.UInt16)]
        [InlineData("ULONG", SQLiteType.Integer, typeof(ulong), DbType.UInt64)]
        [InlineData("UNIQUEIDENTIFIER", SQLiteType.Blob, typeof(Guid), DbType.Guid)]
        [InlineData("VARCHAR", SQLiteType.Text, typeof(string), DbType.String)]
        public void FromDeclaredType_maps_types(string declaredType, int sqliteType, Type clrType, DbType dbType)
        {
            var map = SQLiteTypeMap.FromDeclaredType(declaredType, (SQLiteType)sqliteType);

            Assert.Equal(clrType, map.ClrType);
            Assert.Equal(dbType, map.DbType);
        }

        [Fact]
        public void FromDeclaredType_ignores_facets()
        {
            var map = SQLiteTypeMap.FromDeclaredType("NVARCHAR(4000)", SQLiteType.Text);

            Assert.Equal(typeof(string), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_ignores_case()
        {
            var map = SQLiteTypeMap.FromDeclaredType("int", SQLiteType.Integer);

            Assert.Equal(typeof(int), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_ignores_sqlitetype()
        {
            var map = SQLiteTypeMap.FromDeclaredType("INTEGER", SQLiteType.Text);

            Assert.Equal(typeof(long), map.ClrType);
        }

        [Fact]
        public void FromDeclaredType_falls_back_using_sqlitetype()
        {
            var map = SQLiteTypeMap.FromDeclaredType("UNKNOWN", SQLiteType.Integer);

            Assert.Equal(typeof(long), map.ClrType);
        }

        [Theory]
        [InlineData(SQLiteType.Null, typeof(DBNull))]
        [InlineData(SQLiteType.Integer, typeof(long))]
        [InlineData(SQLiteType.Float, typeof(double))]
        [InlineData(SQLiteType.Text, typeof(string))]
        [InlineData(SQLiteType.Blob, typeof(byte[]))]
        public void FromSQLiteType_maps_types(int sqliteType, Type clrType)
        {
            var map = SQLiteTypeMap.FromSQLiteType((SQLiteType)sqliteType);

            Assert.Equal(clrType, map.ClrType);
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTime()
        {
            var value = new DateTime(2014, 3, 19, 14, 18, 58, 213);

            var map = SQLiteTypeMap.FromClrType<DateTime>();

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("2014-03-19T14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_DateTimeOffset()
        {
            var value = new DateTimeOffset(2014, 3, 19, 14, 18, 58, 213, new TimeSpan(-7, 0, 0));

            var map = SQLiteTypeMap.FromClrType<DateTimeOffset>();

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("2014-03-19T14:18:58.2130000-07:00", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_decimal()
        {
            var value = 3.14m;

            var map = SQLiteTypeMap.FromClrType<decimal>();

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("3.14", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_blob_when_Giud()
        {
            var value = new Guid("36127aab-3769-45b5-8804-f2d447dc001a");

            var map = SQLiteTypeMap.FromClrType<Guid>();

            Assert.Equal(SQLiteType.Blob, map.SQLiteType);
            Assert.Equal(
                new byte[]
                    {
                        0xab, 0x7a, 0x12, 0x36,
                        0x69, 0x37,
                        0xb5, 0x45,
                        0x88, 0x04,
                        0xf2, 0xd4, 0x47, 0xdc, 0x00, 0x1a
                    },
                map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_converts_to_string_when_TimeSpan()
        {
            var value = new TimeSpan(19, 14, 18, 58, 213);

            var map = SQLiteTypeMap.FromClrType<TimeSpan>();

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal("19.14:18:58.2130000", map.ToInterop(value));
        }

        [Fact]
        public void ToInterop_overflows_when_ulong()
        {
            var value = 0xFFFFFFFFFFFFFFFF;

            var map = SQLiteTypeMap.FromClrType<ulong>();

            Assert.Equal(-1L, map.ToInterop(value));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_DateTime()
        {
            var value = new DateTime(2014, 3, 19, 14, 18, 58);

            var map = SQLiteTypeMap.FromClrType<DateTime>();

            Assert.Equal(value, map.FromInterop("2014-03-19 14:18:58"));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_DateTimeOffset()
        {
            var value = new DateTimeOffset(2014, 3, 19, 14, 18, 58, 213, new TimeSpan(-7, 0, 0));

            var map = SQLiteTypeMap.FromClrType<DateTimeOffset>();

            Assert.Equal(value, map.FromInterop("2014-03-19T14:18:58.2130000-07:00"));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_decimal()
        {
            var value = 3.14m;

            var map = SQLiteTypeMap.FromClrType<decimal>();

            Assert.Equal(value, map.FromInterop("3.14"));
        }

        [Fact]
        public void FromInterop_converts_to_blob_when_Giud()
        {
            var value = new Guid("36127aab-3769-45b5-8804-f2d447dc001a");

            var map = SQLiteTypeMap.FromClrType<Guid>();

            Assert.Equal(
                value,
                map.FromInterop(
                    new byte[]
                        {
                            0xab, 0x7a, 0x12, 0x36,
                            0x69, 0x37,
                            0xb5, 0x45,
                            0x88, 0x04,
                            0xf2, 0xd4, 0x47, 0xdc, 0x00, 0x1a
                        }));
        }

        [Fact]
        public void FromInterop_converts_to_string_when_TimeSpan()
        {
            var value = new TimeSpan(14, 18, 58);

            var map = SQLiteTypeMap.FromClrType<TimeSpan>();

            Assert.Equal(SQLiteType.Text, map.SQLiteType);
            Assert.Equal(value, map.FromInterop("14:18:58"));
        }

        [Fact]
        public void FromInterop_overflows_when_ulong()
        {
            var value = 0xFFFFFFFFFFFFFFFF;

            var map = SQLiteTypeMap.FromClrType<ulong>();

            Assert.Equal(value, map.FromInterop(-1L));
        }
    }
}
