using Microsoft.VisualStudio.TestTools.UnitTesting;
using BankMore.ContaCorrente.Domain.Handlers;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using Moq;
using BankMore.ContaCorrente.Domain.Commands;
using System.Threading.Tasks;
using System.Threading;

namespace BankMore.ContaCorrente.Tests;

[TestClass]
public class CadastrarContaCorrenteHandlerTests
{
    private Mock<IContaCorrenteRepository> _mockRepository;
    private CadastrarContaCorrenteHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IContaCorrenteRepository>();
        _handler = new CadastrarContaCorrenteHandler(_mockRepository.Object);
    }

    [TestMethod]
    public async Task Handle_ValidCpf_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CadastrarContaCorrenteCommand
        {
            Cpf = "12345678901",
            Senha = "123456",
            NomeTitular = "João Silva"
        };

        _mockRepository.Setup(x => x.ExisteCpfAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(x => x.ExisteNumeroContaAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Sucesso);
        Assert.IsNotNull(result.Dados);
        _mockRepository.Verify(x => x.InserirAsync(It.IsAny<Domain.Entities.ContaCorrente>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_InvalidCpf_ShouldReturnError()
    {
        // Arrange
        var command = new CadastrarContaCorrenteCommand
        {
            Cpf = "123",
            Senha = "123456",
            NomeTitular = "João Silva"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Sucesso);
        Assert.AreEqual("INVALID_DOCUMENT", result.Erro?.TipoFalha);
    }

    [TestMethod]
    public async Task Handle_ExistingCpf_ShouldReturnError()
    {
        // Arrange
        var command = new CadastrarContaCorrenteCommand
        {
            Cpf = "12345678901",
            Senha = "123456",
            NomeTitular = "João Silva"
        };

        _mockRepository.Setup(x => x.ExisteCpfAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Sucesso);
        Assert.AreEqual("INVALID_DOCUMENT", result.Erro?.TipoFalha);
    }
}
