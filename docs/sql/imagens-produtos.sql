-- Imagens dos produtos (arquivos em src/AgapeApp.Api/wwwroot/imagens/produtos, servidos em /imagens/produtos/...)
-- Requer a migration 0018 (coluna produtos.caminho_imagem): reinicie a API antes de rodar.
-- Créditos e licenças das fotos: src/AgapeApp.Api/wwwroot/imagens/produtos/CREDITOS.md
USE dbAgape;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE produtos SET caminho_imagem = 'imagens/produtos/agua-com-gas.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '61E7233D-4E06-4DB5-B53E-9E66A69025D5'; -- Água com gás 500ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/agua-mineral.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'ACF54734-405A-46F1-9D75-725A85421515'; -- Água mineral 500ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/algodao-doce.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '192274E1-A464-4854-BD5F-793D5329A70E'; -- Algodão-doce
UPDATE produtos SET caminho_imagem = 'imagens/produtos/batata-frita.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '52D2AF92-F3E0-40F8-9739-E60653139343'; -- Batata frita (porção)
UPDATE produtos SET caminho_imagem = 'imagens/produtos/bolo-de-pote.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'DF0E6165-F4F6-46EE-9F8D-6D2047C27843'; -- Bolo de pote
UPDATE produtos SET caminho_imagem = 'imagens/produtos/bone-evento.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'CB0D397E-0B75-45C3-AAB3-D1E698647605'; -- Boné do evento
UPDATE produtos SET caminho_imagem = 'imagens/produtos/brigadeiro.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '16FA1426-97A8-4D29-99A9-7716F96EFCE1'; -- Brigadeiro
UPDATE produtos SET caminho_imagem = 'imagens/produtos/cachorro-quente.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '01A14CA7-D6EF-7D9C-8B5D-FC5C8082188F'; -- cachorro quente
UPDATE produtos SET caminho_imagem = 'imagens/produtos/cachorro-quente.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'E82E4566-1A1E-4368-A462-B37170299E46'; -- Cachorro-quente
UPDATE produtos SET caminho_imagem = 'imagens/produtos/cafe.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '507B2936-802B-4012-A5B0-71C91EEC6C56'; -- Café
UPDATE produtos SET caminho_imagem = 'imagens/produtos/caldo-verde.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'C9863419-8187-481F-BC42-F0823EF3F45C'; -- Caldo verde
UPDATE produtos SET caminho_imagem = 'imagens/produtos/camiseta-evento.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '15DDF5BC-110C-4AA5-9CAD-08CCF6108CC0'; -- Camiseta do evento G
UPDATE produtos SET caminho_imagem = 'imagens/produtos/camiseta-evento.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '5516E0EA-4AE4-4657-8DD2-98A533ED592D'; -- Camiseta do evento GG
UPDATE produtos SET caminho_imagem = 'imagens/produtos/camiseta-evento.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '19B5E8CD-380A-4F58-AA9B-1E5045D3703B'; -- Camiseta do evento M
UPDATE produtos SET caminho_imagem = 'imagens/produtos/camiseta-evento.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '4FDD3E1B-C2D1-449D-A677-B18E371D3386'; -- Camiseta do evento P
UPDATE produtos SET caminho_imagem = 'imagens/produtos/chocolate-quente.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '4043CB60-E029-4640-BA5F-05838B376DC0'; -- Chocolate quente
UPDATE produtos SET caminho_imagem = 'imagens/produtos/coca-cola-2l.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'BD4F056F-A310-4ACB-9D6B-1CAD3125993E'; -- Coca-Cola 2L
UPDATE produtos SET caminho_imagem = 'imagens/produtos/coca-cola-lata.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '6E601575-D1E1-412F-B543-DB7E9918DE17'; -- Coca-Cola lata 350ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/coca-cola-zero-lata.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '80C80CFB-D582-4044-89B3-E9A2BB776DA2'; -- Coca-Cola Zero lata 350ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/coxinha.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '2FF4745A-F05F-4D1A-BBEA-64927F4E1FB4'; -- Coxinha
UPDATE produtos SET caminho_imagem = 'imagens/produtos/suco-uva.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '79C0E264-9C24-4AE7-8D45-500CF04F2453'; -- Del Valle uva 290ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/esfiha.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '8338C6E9-DE53-4006-A0FB-58D97793A9F2'; -- Esfiha de carne
UPDATE produtos SET caminho_imagem = 'imagens/produtos/espetinho-carne.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '4215C178-ADB1-4288-B3C5-461111018CE7'; -- Espetinho de carne
UPDATE produtos SET caminho_imagem = 'imagens/produtos/espetinho-frango.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'D4EF1973-04B8-41E0-8939-7CB55411A76B'; -- Espetinho de frango
UPDATE produtos SET caminho_imagem = 'imagens/produtos/fanta-laranja.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '5AC7A3E3-2723-499B-A254-998D40FB0832'; -- Fanta Laranja lata 350ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/guarana-antarctica.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '82E395D7-8E02-4BFF-9DA0-E417722179AC'; -- Guaraná Antarctica lata 350ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/refrigerante-limao.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'A8141F59-0EED-4C7A-BC66-4CC3849971F3'; -- H2OH! Limão 500ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/maca-do-amor.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'FE82CB67-D765-4A40-A133-3CCD9073E14D'; -- Maçã do amor
UPDATE produtos SET caminho_imagem = 'imagens/produtos/milho-verde.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '9720DABA-5E71-4CAF-878C-C4765F3D041D'; -- Milho verde
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pacoca.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '27D73D88-4F32-4200-A53A-BF0A31900AE7'; -- Paçoca
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pastel.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '01A14CA9-AFC4-7458-8A95-904268CAAA43'; -- Pastel
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pastel.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'DB0D1BF5-646F-454A-B6DE-99E075FEBF22'; -- Pastel de carne
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pastel.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'EC975E37-FFED-442B-B2A3-43797506D8E6'; -- Pastel de frango com catupiry
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pastel.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'DA7CCC93-FC4B-458D-89BC-EC4CE865B628'; -- Pastel de palmito
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pastel.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '293713EB-9762-4B66-887F-4B772E58F10E'; -- Pastel de queijo
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pe-de-moleque.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'BC8CB990-2F5E-4166-ACFB-8EED437DEF3E'; -- Pé de moleque
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pipoca.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '97A3A362-A710-46CD-B3D6-2C66472E032E'; -- Pipoca
UPDATE produtos SET caminho_imagem = 'imagens/produtos/pizza-fatia.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'ECE9FD9E-EA17-4DED-97F8-5A78E782DF02'; -- Pizza (fatia)
UPDATE produtos SET caminho_imagem = 'imagens/produtos/quentao.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '9732135B-E1FD-407C-B1D5-645384364B03'; -- Quentão
UPDATE produtos SET caminho_imagem = 'imagens/produtos/refrigerante-garrafa.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '552DE28C-D097-47CB-8375-DF71A99D9AB0'; -- Refrigerante 600ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/sanduiche-natural.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '2EA0C4AC-476A-42AE-8460-55920FC07A00'; -- Sanduíche natural
UPDATE produtos SET caminho_imagem = 'imagens/produtos/sprite.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = 'A4769DFE-44F5-4532-B8F9-C019166B9DEE'; -- Sprite lata 350ml
UPDATE produtos SET caminho_imagem = 'imagens/produtos/suco-laranja.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '4DCDDC21-71F3-4AF9-9437-B1452932C148'; -- Suco de laranja natural
UPDATE produtos SET caminho_imagem = 'imagens/produtos/terco.jpg', atualizado_em = SYSDATETIMEOFFSET() WHERE id = '24842E70-EC77-49B9-9A22-9AD2586B095D'; -- Terço

SELECT nome, caminho_imagem FROM produtos ORDER BY nome;

COMMIT TRANSACTION;
