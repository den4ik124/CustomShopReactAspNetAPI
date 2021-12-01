﻿using System;
using System.Linq;

namespace OrmRepositoryUnitOfWork.Attributes
{
    public class TypeAttribute : Attribute
    {
        private Type baseType;

        public int TypeID { get; set; }
        public Type Type { get; set; }
        public Type BaseType { get => baseType; set => baseType = value; }
        public string BaseTypeName { get => this.baseType.Name; }

        public string ColumnMatching { get; set; }

        public TypeAttribute()
        {
        }
    }
}