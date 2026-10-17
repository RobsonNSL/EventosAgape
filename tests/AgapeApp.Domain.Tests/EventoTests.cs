using AgapeApp.Domain.Entities;
using AgapeApp.Domain.Enums;
using AgapeApp.Domain.Exceptions;

namespace AgapeApp.Domain.Tests;

public class EventoTests
{
    private static readonly DateOnly Inicio = new(2026, 10, 17);

    private static Evento CriarEventoValido() =>
        Evento.Criar("Festa da igreja", Inicio, Inicio, Guid.NewGuid());

    [Fact]
    public void Criar_DeveIniciarEmRascunho()
    {
        var evento = CriarEventoValido();

        Assert.Equal(SituacaoEvento.Rascunho, evento.Situacao);
        Assert.False(evento.EstaAberto());
        Assert.False(evento.EstaEncerrado());
    }

    [Fact]
    public void Criar_ComDataFimAnterior_DeveLancarRegraNegocioException()
    {
        Assert.Throws<RegraNegocioException>(() =>
            Evento.Criar("Festa", Inicio, Inicio.AddDays(-1), Guid.NewGuid()));
    }

    [Fact]
    public void Abrir_DeRascunho_DeveVirarAberto()
    {
        var evento = CriarEventoValido();

        evento.Abrir();

        Assert.True(evento.EstaAberto());
    }

    [Fact]
    public void Abrir_QuandoJaEstaAberto_DeveLancarRegraNegocioException()
    {
        var evento = CriarEventoValido();
        evento.Abrir();

        Assert.Throws<RegraNegocioException>(() => evento.Abrir());
    }

    [Fact]
    public void Encerrar_DeAberto_DeveVirarEncerrado()
    {
        var evento = CriarEventoValido();
        evento.Abrir();

        evento.Encerrar();

        Assert.True(evento.EstaEncerrado());
    }

    [Fact]
    public void Encerrar_EmRascunho_DeveLancarRegraNegocioException()
    {
        var evento = CriarEventoValido();

        Assert.Throws<RegraNegocioException>(() => evento.Encerrar());
    }

    [Fact]
    public void Atualizar_QuandoEncerrado_DeveLancarRegraNegocioException()
    {
        var evento = CriarEventoValido();
        evento.Abrir();
        evento.Encerrar();

        Assert.Throws<RegraNegocioException>(() =>
            evento.Atualizar("Outro nome", Inicio, Inicio));
    }
}
