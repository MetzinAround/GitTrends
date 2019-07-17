﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitTrends.Shared;
using SQLite;

namespace GitTrends
{
    abstract class RepositoryDatabase : BaseDatabase
    {
        #region Methods
        public static async Task<int> SaveRepository(Repository repository)
        {
            var databaseConnection = await GetDatabaseConnectionAsync<RepositoryDatabaseModel>().ConfigureAwait(false);

            return await databaseConnection.InsertOrReplaceAsync((RepositoryDatabaseModel)repository).ConfigureAwait(false);
        }

        public static async Task<int> SaveRepositories(IEnumerable<Repository> repositories)
        {
            try
            {
                var databaseConnection = await GetDatabaseConnectionAsync<RepositoryDatabaseModel>().ConfigureAwait(false);

                var repositoryDatabaseModels = repositories.Select(x => (RepositoryDatabaseModel)x);

                return await databaseConnection.InsertAllAsync(repositoryDatabaseModels).ConfigureAwait(false);
            }
            catch(SQLiteException e) when (e.Result is SQLite3.Result.Constraint)
            {
                return -1;
            }
        }

        public static async Task<Repository> GetRepository(Uri repositoryUri)
        {
            var databaseConnection = await GetDatabaseConnectionAsync<RepositoryDatabaseModel>().ConfigureAwait(false);

            var repositoryDatabaseModel = await databaseConnection.GetAsync<RepositoryDatabaseModel>(repositoryUri).ConfigureAwait(false);

            return (Repository)repositoryDatabaseModel;
        }

        public static async Task<IEnumerable<Repository>> GetRepositories()
        {
            var databaseConnection = await GetDatabaseConnectionAsync<RepositoryDatabaseModel>().ConfigureAwait(false);

            var repositoryDatabaseModels = await databaseConnection.Table<RepositoryDatabaseModel>().ToListAsync().ConfigureAwait(false);

            return repositoryDatabaseModels.Select(x => (Repository)x);
        }
        #endregion

        #region Classes
        [EditorBrowsable(EditorBrowsableState.Never)]
        class RepositoryDatabaseModel : IRepository
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public long ForkCount { get; set; }

            [PrimaryKey]
            public Uri Uri { get; set; }

            public int StarCount { get; set; }

            public string OwnerLogin { get; set; }

            public Uri OwnerAvatarUrl { get; set; }

            public int IssuesCount { get; set; }

            public static explicit operator Repository(RepositoryDatabaseModel repositoryDatabaseModel)
            {
                return new Repository(repositoryDatabaseModel.Name, repositoryDatabaseModel.Description,
                    repositoryDatabaseModel.ForkCount, new RepositoryOwner(repositoryDatabaseModel.OwnerLogin, repositoryDatabaseModel.OwnerAvatarUrl), new IssuesConnection(repositoryDatabaseModel.IssuesCount, null),
                    repositoryDatabaseModel.Uri, new StarGazers(repositoryDatabaseModel.StarCount));
            }

            public static implicit operator RepositoryDatabaseModel(Repository repository)
            {
                return new RepositoryDatabaseModel
                {
                    Description = repository.Description,
                    StarCount = repository.StarCount,
                    Uri = repository.Uri,
                    IssuesCount = repository.IssuesCount,
                    ForkCount = repository.ForkCount,
                    Name = repository.Name,
                    OwnerAvatarUrl = repository.OwnerAvatarUrl,
                    OwnerLogin = repository.OwnerLogin
                };
            }
        }
        #endregion
    }
}