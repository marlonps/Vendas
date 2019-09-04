# Vendas
Sistema de processamento de dados de vendas

# Introdução
Esta aplicação console monitora continuamente um diretório de entrada de arquivos; uma vez que um ou mais arquivos sejam criados nesse diretório, a aplicação lê cada um dos arquivos, parseia o conteúdo em classes pré-definidas, e gera um arquivo de saída contendo as informações referentes à entrada.

# Dependências
Este projeto não possui nenhuma dependência externa; o desenvolvimento foi pensado em um ambiente onde o usuário possui pouca ou nenhuma autonomia/capacidade técnica/vontade de instalar outras aplicações.

# Build e funcionamento
Basta executar o projeto a partir de qualquer lugar da máquina; a aplicação verifica a pasta local do usuário e checa se existe a pasta "data", com as subpastas "in" e "out"; se não existirem tais pastas, a aplicação cria as mesmas.

À medida que arquivos vão sendo criados na pasta "in", estes vão sendo processados pela aplicação e movidos para um diretório "Processados". Cada arquivo gera um arquivo de saída na pasta "out". 

A especificação menciona "processamemento em lote"; contudo, todas as demais descrições tratam como se o arquivo de saída fosse correspondente a um único arquivo de entrada, e não a um conjunto; esta última visão foi a escolhida para o projeto.

Cada arquivo é carregado em memória de forma incremental e parseado de forma paralela. As operações potencialmente custosas que contém laços são paralelizadas, para aproveitar-se de processamento multithread. A cada linha, é feita a identificação do objeto representado, o parsing e a atualização das variaveis de saída - isso foi pensado para evitar-se operações sobre uma lista com muitos elementos. No caso do pior vendedor, é necessário que o vendedor já tenha sido previamente declarado. Para agilizar a pesquisa, é criado um dicionario concorrente que armazena o nome do vendedor e o total de vendas, sendo este total incrementado a cada registro pertinente.

# Melhorias, dificuldades e limitações
Para detectar alterações no diretório de entrada, é recomendado usar a classe "SystemFileWatcher". Contudo, não foi possível implementá-la de forma consistente.

Em questão de performance, carregar as linhas em um array é mais eficiente do que usar uma lista; contudo, como não sabemos o tamanho máximo de linhas que os arquivos podem ter, não é possível pré-alocar memória.

Para manipular arquivos muito grandes, é recomendado usar a classe "MemoryMappedFile". Contudo, não foi possível implementá-la de forma consistente. Outras alternativas seriam usar ferramentas de cache (como memcached ou Redis), ou um banco Sqlite para guardar temporariamente os registros.

As ações de parsing foram implementadas de acordo com o especificado; contudo, os objetos resultantes não são armazenados em algum banco de dados ou similar. Caso desejar-se ganhar velocidade, os parsings podem ser omitidos ou reduzidos (apenas dando split nas linhas, em vez de checar os tipos).

Como mencionado anteriormente, esta solução foi desenvolvida pensando em um mínimo de configuração por parte do usuário. Uma solução alternativa seria o uso do Filebeats em conjunto com o Logstash. Nesse cenário, o Filebeats monitoraria a criação de arquivos no diretório de entrada, e os transmitiria para o Logstash. Esse, por sua vez, teria o trabalho de receber tais arquivos com input, parseá-los através de uma cláusula "filter" usando a sitnaxe do grok, e gerar os acumuladores através de aggregates; tais acumuladores seriam escritos no arquivo de saída, que seria o output do Logstash.
