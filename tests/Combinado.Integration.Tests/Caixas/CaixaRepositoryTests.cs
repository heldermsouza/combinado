using Combinado.Application.Caixas;
using Combinado.Application.Common;
using Combinado.Domain.Caixas;
using Combinado.Domain.ValueObjects;
using Combinado.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Combinado.Integration.Tests.Caixas;

public sealed class CaixaRepositoryTests(CombinadoApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly NumeroWhatsApp Ana = NumeroWhatsApp.Criar("11911111111").Value;
    private static readonly NumeroWhatsApp Bruno = NumeroWhatsApp.Criar("21922222222").Value;

    [Fact]
    public async Task Persiste_e_recarrega_o_agregado_completo()
    {
        var ct = TestContext.Current.CancellationToken;
        Caixa original;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, uow, clock) = Resolve(scope);
            original = Caixa.Criar("Ana", Ana, clock).Value;
            original.ConvidarParceiro("Bruno", Bruno, clock);
            repo.Add(original);
            await uow.SaveChangesAsync(ct);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, _, _) = Resolve(scope);
            var carregado = await repo.GetByIdAsync(original.Id, ct);

            Assert.NotNull(carregado);
            Assert.Equal(StatusCaixa.Pendente, carregado.Status);
            Assert.Equal(2, carregado.Parceiros.Count);
            Assert.Equal(Categoria.Padroes.Count, carregado.Categorias.Count);
            Assert.Equal("Ana", carregado.Fundador.Nome);
            Assert.Equal(Bruno, carregado.Convidado!.NumeroWhatsApp);
            Assert.False(carregado.Convidado.Vinculado);
        }
    }

    [Fact]
    public async Task Vincular_e_salvar_persiste_status_ativo()
    {
        var ct = TestContext.Current.CancellationToken;
        Caixa caixa;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, uow, clock) = Resolve(scope);
            caixa = Caixa.Criar("Ana", Ana, clock).Value;
            caixa.ConvidarParceiro("Bruno", Bruno, clock);
            repo.Add(caixa);
            await uow.SaveChangesAsync(ct);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, uow, clock) = Resolve(scope);
            var carregado = (await repo.GetByNumeroWhatsAppAsync(Bruno, ct))!;
            Assert.True(carregado.VincularParceiro(Bruno, clock).IsSuccess);
            await uow.SaveChangesAsync(ct);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, _, _) = Resolve(scope);
            var relido = await repo.GetByIdAsync(caixa.Id, ct);

            Assert.Equal(StatusCaixa.Ativo, relido!.Status);
            Assert.True(relido.Convidado!.Vinculado);
        }
    }

    [Fact]
    public async Task RN01_numero_ja_usado_em_outro_caixa_e_detectado_e_barrado_pelo_indice()
    {
        var ct = TestContext.Current.CancellationToken;

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var (repo, uow, clock) = Resolve(scope);
            repo.Add(Caixa.Criar("Ana", Ana, clock).Value);
            await uow.SaveChangesAsync(ct);
        }

        await using var segundo = Factory.Services.CreateAsyncScope();
        var (repo2, uow2, clock2) = Resolve(segundo);

        Assert.True(await repo2.NumeroJaVinculadoAsync(Ana, ct));
        Assert.False(await repo2.NumeroJaVinculadoAsync(Bruno, ct));

        // Se um handler ignorar a checagem acima, o banco ainda barra.
        var outroCaixa = Caixa.Criar("Impostora", Ana, clock2).Value;
        repo2.Add(outroCaixa);
        await Assert.ThrowsAsync<DbUpdateException>(() => uow2.SaveChangesAsync(ct));
    }

    private static (ICaixaRepository Repo, IUnitOfWork Uow, TimeProvider Clock) Resolve(AsyncServiceScope scope) =>
        (scope.ServiceProvider.GetRequiredService<ICaixaRepository>(),
         scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
         scope.ServiceProvider.GetRequiredService<TimeProvider>());
}
