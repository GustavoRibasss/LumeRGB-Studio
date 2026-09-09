# ThebestRGB

Controle a iluminação do teclado e do gabinete em um só aplicativo para Windows.

## Como usar

Abra `ThebestRGB.exe`, selecione os dispositivos, escolha uma cor ou efeito e clique em **Aplicar iluminação**. Alterar uma opção prepara a configuração; o envio acontece ao aplicar.

O aplicativo permite uma instância por usuário e sessão. Ao abrir novamente, ele mostra a janela existente. O menu da bandeja permite configurar o botão X para minimizar; use **Sair** para encerrar.

## Equipamentos compatíveis deste projeto

- AULA HERO 68, incluindo a barra LED.
- MSI B650M PROJECT ZERO e seus conectores ARGB.
- Logo da Gigabyte RTX 3080 Vision compatível com o controlador implementado.
- Dois módulos Patriot Viper com controlador ENE, acessados pelo OpenRGB.

O suporte foi desenvolvido para esse conjunto. Outros modelos podem usar protocolos diferentes. O aplicativo controla iluminação, sem ajustar a velocidade das ventoinhas.

## Personalização

Em **Predefinições**, passe o mouse sobre um efeito para abrir suas combinações. **Configurar efeito** permite ajustar cores, intensidade e movimento.

O seletor preserva o RGB escolhido. **Editar / cores salvas** abre a paleta personalizada, que é guardada entre execuções. Use o **Conta-gotas** para escolher um ponto da tela e consultar RGB e hexadecimal. Esc cancela.

Na barra LED, **Acompanhar as teclas** usa a cor das teclas em modo constante, sem ativar o efeito nativo Fluxo.

### Cores da tela

Usa o monitor principal e estima a cor predominante. As transições são graduais; diminuir a velocidade deixa as mudanças mais lentas.

A chave **Luminosidade da tela**, abaixo de Velocidade, permite escolher:

- Ligada: acompanha também a luminosidade da imagem.
- Desligada: acompanha a cor com o brilho definido pelo usuário. Uma tela preta conserva a última cor.

As capturas ficam apenas na memória. O tom dos LEDs pode diferir do monitor. Conteúdo protegido e alguns modos de exibição podem impedir a captura correta.

## Inicialização e arquivos salvos

Ao aplicar, o aplicativo salva a configuração para tentar restaurá-la na próxima abertura. A RAM conecta em paralelo, e falhas podem receber novas tentativas. O Windows pode solicitar autorização para iniciar o OpenRGB.

Abrir com o Windows exige registrar a inicialização no computador. Copiar o executável para outra máquina não configura isso automaticamente. O aplicativo não garante iluminação antes do login nem gravação na memória interna dos dispositivos.

Perfis, paleta, última configuração e calibração ficam em `%LOCALAPPDATA%\ThebestRGB`. Na primeira abertura, os dados da versão antiga são copiados para a nova pasta, sem apagar os originais.

A consulta automática ao GitHub avisa sobre versões publicadas; não instala atualizações. Um commit novo na branch principal não equivale a uma nova versão publicada.

## Calibração

**Ferramentas → Ajustar cores entre dispositivos** permite ajustar os canais RGB. Existe também uma correção seletiva de vermelho, habilitada por dispositivo na configuração local. Ela usa uma referência visual específica e não substitui calibração com instrumentos. A calibração pessoal não é distribuída no repositório.

## Compilar

No PowerShell, na pasta do projeto:

```powershell
.\compilar.ps1
```

O script usa o compilador do .NET Framework do Windows e gera `ThebestRGB.exe`, com ícone e componentes OpenRGB incorporados.

Os nomes técnicos dos arquivos e classes foram preservados para manter compatibilidade com scripts e testes. A documentação e os textos destinados ao usuário devem usar português claro.

## Testes e contribuições

A pasta `tests` contém verificações de perfis, restauração, interface e seleção de cores. Testes simulados não comprovam o tom físico dos LEDs; isso exige comparação no equipamento.

Use novos commits em português, descrevendo a mudança, por exemplo: `Corrige o salvamento das cores personalizadas`. O histórico publicado foi preservado.

Consulte [os avisos de terceiros](THIRD-PARTY-NOTICES.md) e a [licença](LICENSE.txt).
