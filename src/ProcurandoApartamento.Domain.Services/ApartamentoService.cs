using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using ProcurandoApartamento.Domain.Services.Interfaces;
using ProcurandoApartamento.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using LanguageExt;
using System.Collections.Generic;

namespace ProcurandoApartamento.Domain.Services
{
    public class ApartamentoService : IApartamentoService
    {
        protected readonly IApartamentoRepository _apartamentoRepository;

        public ApartamentoService(IApartamentoRepository apartamentoRepository)
        {
            _apartamentoRepository = apartamentoRepository;
        }

        public virtual async Task<Apartamento> Save(Apartamento apartamento)
        {
            await _apartamentoRepository.CreateOrUpdateAsync(apartamento);
            await _apartamentoRepository.SaveChangesAsync();
            return apartamento;
        }

        public virtual async Task<IPage<Apartamento>> FindAll(IPageable pageable)
        {
            var page = await _apartamentoRepository.QueryHelper()
                .GetPageAsync(pageable);
            return page;
        }

        public virtual async Task<Apartamento> FindOne(long id)
        {
            var result = await _apartamentoRepository.QueryHelper()
                .GetOneAsync(apartamento => apartamento.Id == id);
            return result;
        }

        public virtual async Task Delete(long id)
        {
            await _apartamentoRepository.DeleteByIdAsync(id);
            await _apartamentoRepository.SaveChangesAsync();
        }

        public virtual async Task<string> MelhorApartamento(string[] estabelecimentos)
        {
            var apartamentos = await _apartamentoRepository.GetAllAsync();
            var apartamentosDisponiveis = apartamentos.Where(x => x.ApartamentoDisponivel);

            var aptos = new List<Apartamento>();
            foreach (var estabelecimento in estabelecimentos)
            {
                var apartamentosProximos = apartamentosDisponiveis.Where(x => x.Estabelecimento.Equals(estabelecimento) && x.EstabelecimentoExiste);
                aptos.AddRange(apartamentosProximos);
            }

            if (!aptos.Any())
            {
                return $"Quadra {apartamentosDisponiveis.LastOrDefault().Quadra}";
            }
            else if (aptos.Count() == 1)
            {
                return $"Quadra {aptos.First().Quadra}";
            }
            else
            {
                if (estabelecimentos.Count() > 1)
                {
                    var quadra = aptos
                        .GroupBy(x => x.Quadra)
                        .OrderByDescending(g => g.Count())
                        .SelectMany(g => g)
                        .FirstOrDefault().Quadra;
                    return $"Quadra {quadra}";
                }
                else
                {
                    return $"Quadra {aptos.LastOrDefault().Quadra}";
                }
            }
        }
    }
}
