﻿using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Bisoyi.DB
{
    public static class DbModificationClauseExtensions
    {
        public static DbModificationClause UpdateIfMatch(
            this DbModificationClause clause,
            string property,
            DbExpression value)
        {
            return clause.IsFor(property)
                    ? DbExpressionBuilder.SetClause(clause.Property(), value)
                    : clause;
        }

        public static bool IsFor(
            this DbModificationClause clause,
            string property)
        {
            return clause.HasPropertyExpression()
                && clause.Property().Property.Name == property;
        }

        public static DbPropertyExpression Property(
            this DbModificationClause clause)
        {
            if (clause.HasPropertyExpression())
            {
                var setClause = (DbSetClause)clause;
                return (DbPropertyExpression)setClause.Property;
            }

            var message =
                "clause does not contain property expression. " +
                "Use HasPropertyExpression method to check if it has property expression.";
            throw new Exception(message);
        }

        public static bool HasPropertyExpression(
            this DbModificationClause modificationClause)
        {
            var setClause = modificationClause as DbSetClause;
            return setClause?.Property is DbPropertyExpression;
        }
    }

}
