using Application.Exceptions;
using AutoMapper;
using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class GenericService<TVM, TEntity> : IGenericService<TVM, TEntity>

        where TVM : class
        where TEntity : BaseEntity, new()
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly IMapper _mapper;
        private readonly IUnityOfWork _unityOfWork;
        private readonly ILogger<GenericService<TVM, TEntity>> _logger;

        public GenericService(
            IMapper mapper,
            IGenericRepository<TEntity> repository,
            IUnityOfWork unityOfWork,
            ILogger<GenericService<TVM, TEntity>> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _unityOfWork = unityOfWork;
            _logger = logger;
        }

        public async Task<TVM> AddAsync(TVM entity)
        {
            if (entity == null)
                throw new NotNullExceptions();

            var entityToAdd = _mapper.Map<TEntity>(entity);
            var result = await _repository.AddAsync(entityToAdd);
            await _unityOfWork.SaveChangesAsync();

            _logger.LogInformation("Added {EntityType} with ID: {EntityId}", typeof(TEntity).Name, result.Id);
            _logger.LogInformation("MongoDB Test Log: Added entity at {Time}", DateTime.UtcNow);

            return _mapper.Map<TVM>(result);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new NotNullExceptions();

            await _repository.DeleteAsync(id);
            await _unityOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted {EntityType} with ID: {EntityId}", typeof(TEntity).Name, id);
            return true;
        }

        public async Task<IEnumerable<TVM>> GetAllAsync(
            Func<IQueryable<TEntity>, IQueryable<TEntity>> include = null)
        {
            var data = await _repository.GetAllAsync(include);

            if (data == null)
                return Enumerable.Empty<TVM>();

            _logger.LogInformation("Retrieved all {EntityType} entities", typeof(TEntity).Name);
            return _mapper.Map<IEnumerable<TVM>>(data);
        }

        public async Task<TVM> GetByIdAsync(int id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
        {
            var entity = await _repository.GetByIdAsync(id, include);
            if (entity == null)
                throw new NotNullExceptions();

            _logger.LogInformation("Retrieved {EntityType} with ID: {EntityId}", typeof(TEntity).Name, id);
            return _mapper.Map<TVM>(entity);
        }

        public async Task<TVM> UpdateAsync(TVM entity)
        {
            if (entity == null)
                throw new NotNullExceptions();

            var mapped = _mapper.Map<TEntity>(entity);
            await _repository.UpdateAsync(mapped);
            await _unityOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated {EntityType} with ID: {EntityId}", typeof(TEntity).Name, mapped.Id);
            return _mapper.Map<TVM>(mapped);
        }
    }
}
