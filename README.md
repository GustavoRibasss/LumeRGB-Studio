# Lume Studio 22

Abra `LumeRGB-Studio-v22.exe` diretamente. A distribuição compacta funciona com um único arquivo; a cor inicial e a restauração do setup continuam em branco, brilho 100%.

## As 15 melhorias

1. **Prévia da barra:** a barra no teclado e na janela Barra LED representa o modo escolhido. A animação é ilustrativa; o firmware pode apresentar diferenças.
2. **Barra nos perfis:** salvar, carregar, duplicar, exportar e importar preservam modo, cor, multicolorido, brilho e velocidade da barra. Carregar apenas prepara os ajustes.
3. **Barra independente:** Aplicar à barra mantém os efeitos dos demais dispositivos em execução. Os envios USB da barra e das teclas são coordenados para não se sobreporem.
4. **Perfis rápidos:** clique com o botão direito em um perfil e marque como favorito. Os três primeiros favoritos aparecem abaixo da prévia. Um clique carrega e aplica o perfil aos dispositivos selecionados nele.
5. **Comparar:** o botão no canto superior da prévia abre a última configuração enviada e os novos ajustes lado a lado, incluindo a barra. Disponível depois do primeiro envio bem-sucedido.
6. **Alterações pendentes:** um ponto amarelo no cartão indica ajustes ainda não aplicados. A marca é removida após o envio correspondente. Passe o mouse no cartão para ver a indicação.
7. **Nomes dos equipamentos:** AULA HERO 68, MSI 650M PROJECT ZERO, RTX 3080 VISION e VIPER 7000MHZ aparecem com os nomes corretos deste setup. Os nomes não significam que a conexão foi verificada: consulte o estado logo abaixo.
8. **Efeitos favoritos:** clique na estrela ao lado do modo. Os favoritos aparecem primeiro e são lembrados ao reabrir.
9. **Copiar paleta:** selecione o efeito de destino e abra Ferramentas → Copiar paleta. Escolha a origem. Cores personalizadas são copiadas diretamente; para paletas padrão, são usadas cores representativas do efeito. Movimento, direção e quantidade de pulsos do destino são preservados.
10. **Apagar e restaurar:** Apagar tudo interrompe a animação e tenta apagar os quatro dispositivos e a barra, sem trocar os ajustes da prévia. Restaurar luzes recupera a configuração que estava ativa. Dispositivos sem envio anterior usam os ajustes atuais como referência. Falhas são informadas, e a restauração pode ser tentada novamente. Fechar o app encerra essa memória temporária de restauração.
11. **Exportar/importar perfis:** em Ferramentas. A importação valida todo o arquivo antes de salvar. Nomes repetidos recebem um número; os existentes são preservados. Perfis antigos sem configuração da barra usam Acompanhar as teclas.
12. **Correção de cores por dispositivo:** Ferramentas → Ajustar cores entre dispositivos. Reduza vermelho, verde ou azul para aproximar os tons. 100% mantém o canal original. Salve e aplique a iluminação para usar. A correção fica no computador, separada dos perfis. Cores internas dos modos multicoloridos nativos da barra dependem do firmware.
13. **Zonas ARGB da MSI:** no card **MSI 650M PROJECT ZERO**, clique em **Zonas ARGB** para separar Fans, Water Cooler e o terceiro canal. Escolha o canal físico e a cor de cada um, salve e depois clique em **Aplicar** no card da placa-mãe. Se os equipamentos estiverem ligados no mesmo header, o hardware recebe um único sinal e não permite separação por software.
14. **Acabamento da janela:** a moldura superior agora acompanha o tema escuro, com controles de minimizar, maximizar e fechar. Os botões Comparar e Desmarcar todos ganharam margem para não encostar ou cortar na borda.
15. **Configuração completa:** Ferramentas → Exportar configuração completa guarda perfis, setup atual, favoritos, correções de cor e zonas ARGB. Importar configuração completa restaura tudo em um único arquivo JSON, sem alterar o executável portable.

## Instalação opcional

O executável pode ser usado diretamente. Os perfis ficam em `%LOCALAPPDATA%\LumeRGB`.

## Validação

Testes simulados cobrem perfis antigos, importação com conflitos e dados inválidos, salvar/carregar a barra, favoritos, aplicação rápida, cópia de paleta, alterações pendentes, troca da barra durante a animação, falhas de envio, apagar/restaurar, correção RGB e pacotes MSI com cores separadas por canal. Também foram conferidas as janelas e os layouts existentes em escalas sintéticas de 100%, 125% e 150%.

Nenhum comando foi enviado aos dispositivos físicos durante os testes. O comportamento visual real dos LEDs precisa ser conferido no setup.
