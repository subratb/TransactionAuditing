using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace Bisoyi.DB
{
    public class AuditInterceptor : IDbCommandTreeInterceptor
    {
        public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
        {
            if (interceptionContext.OriginalResult.DataSpace != DataSpace.SSpace)
            {
                return;
            }

            var securityContext = SecurityContext.Current;
            var currentTime = DateTime.Now;

            var insertCommand = interceptionContext.Result as DbInsertCommandTree;
            if (insertCommand != null)
            {
                interceptionContext.Result = AuditInsertCommand(insertCommand,securityContext,currentTime);
            }

            var updateCommand = interceptionContext.OriginalResult as DbUpdateCommandTree;
            if (updateCommand != null)
            {
                interceptionContext.Result = AuditUpdateCommand(updateCommand, securityContext, currentTime);
            }
        }

        private static DbCommandTree AuditInsertCommand(DbInsertCommandTree insertCommand,SecurityContext securityContext, DateTime currentTime)
        {
            var now = DateTime.Now;

            var setClauses = insertCommand.SetClauses
                .Where(clause => !clause.IsFor("ModUser")) 
                .Where(clause => !clause.IsFor("ModDate")) 
                .Select(clause => clause.UpdateIfMatch("AddUser", DbExpression.FromString(securityContext.User)))
                .Select(clause => clause.UpdateIfMatch("AddDate", DbExpression.FromDateTime(currentTime)))                
                .ToList();

            return new DbInsertCommandTree(
                insertCommand.MetadataWorkspace,
                insertCommand.DataSpace,
                insertCommand.Target,
                setClauses.AsReadOnly(),
                insertCommand.Returning);
        }

        private static DbCommandTree AuditUpdateCommand(DbUpdateCommandTree updateCommand, SecurityContext securityContext, DateTime currentTime)
        {
            //Update ModUser and ModDate values
            //Remove AddUser and AddDate fields from UPDATE COMMAND
            var setClauses = updateCommand.SetClauses
                .Where(clause => !clause.IsFor("AddUser")) 
                .Where(clause => !clause.IsFor("AddDate")) 
                .Select(clause => clause.UpdateIfMatch("ModUser", DbExpression.FromString(securityContext.User)))
                .Select(clause => clause.UpdateIfMatch("ModDate", DbExpression.FromDateTime(currentTime)))
                .ToList();

            return new DbUpdateCommandTree(
                updateCommand.MetadataWorkspace,
                updateCommand.DataSpace,
                updateCommand.Target,
                updateCommand.Predicate,
                setClauses.AsReadOnly(), null);
        }
    }
}
