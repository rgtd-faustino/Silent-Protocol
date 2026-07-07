# Capa

Instituto Superior de Engenharia de Lisboa (ISEL)  
Departamento de Engenharia Informática (DEI)  
LEIM \- Licenciatura em Engenharia Informática e Multimédia

Silent Protocol e o Desenvolvimento de um Jogo de Stealth e Espionagem Corporativa

Nome do Aluno: João Póvoa 51392  
Nome do Aluno: Rafael Faustino 51394

Orientadores: Professor Hugo Cordeiro
Arguente: Professor João Beleza

Junho, 2026

# Resumo

O presente relatório documenta o desenvolvimento do Silent Protocol, um videojogo de infiltração e resolução de puzzles em primeira pessoa, ambientado num cenário de espionagem corporativa e cibersegurança. O objetivo do projeto consistiu na conceção e implementação de um protótipo funcional que combina mecânicas de furtividade social, recolha de informação e minijogos temáticos inspirados em técnicas reais de segurança informática, como a captura de pacotes de rede, a desencriptação simétrica e a engenharia social. O jogador assume o papel de um infiltrado num edifício corporativo de cinco pisos onde deve cumprir tarefas de trabalho diárias enquanto recolhe, de forma discreta, evidências de um esquema de exfiltração de dados. Os principais contributos incluem o sistema de rotas de NPC com rotinas dinâmicas distribuídas pelos cinco pisos do edifício, o sistema duplo de suspeita que separa a confiança operacional da empresa (*company awareness*) da suspeita comportamental geral, e a integração de cinco minijogos que traduzem conceitos de cibersegurança para mecânicas de jogo acessíveis a jogadores sem formação técnica. O protótipo foi submetido a testes funcionais e a sessões de testes de jogabilidade e usabilidade com utilizadores externos, cujos resultados orientaram o refinamento das mecânicas ao longo do desenvolvimento. Foi produzido um protótipo completo e funcional que cumpre os objetivos estabelecidos no início do projeto.

# Abstract

This report documents the development of Silent Protocol, a first-person stealth and puzzle-solving video game set in a corporate espionage and cybersecurity scenario. The project aimed to design and implement a functional prototype combining social stealth mechanics, information gathering, and thematic minigames inspired by real-world information security techniques such as network packet capture, symmetric decryption, and social engineering. The player assumes the role of an infiltrator in a five-floor corporate building, required to complete daily work tasks while discreetly collecting evidence of a data exfiltration scheme. The main contributions include the NPC routing system with dynamic routines across five floors, the dual suspicion system separating operational company trust (company awareness) from general behavioural suspicion, and the integration of five minigames that translate cybersecurity concepts into game mechanics accessible to players without technical background. The prototype was validated through functional tests and play testing sessions with external users, whose results guided the refinement of game mechanics throughout development. A complete and functional prototype was produced that fulfils the objectives outlined at the beginning of the project.

# Agradecimentos

Ao orientador, Professor Hugo Cordeiro, pela disponibilidade constante e pelas sugestões técnicas que acompanharam o desenvolvimento do Silent Protocol ao longo de todo o semestre. Ao Professor João Beleza, por ter aceite ser arguente na defesa deste projeto. A todos os colegas e amigos que testaram as diversas versões do jogo e cujas opiniões permitiram o refinamento progressivo das mecânicas. Às nossas famílias, pelo apoio demonstrado ao longo do percurso letivo. À Mariana Banha e à Teresa Ramos, pelo incentivo durante o processo de desenvolvimento.

# Índice

# Lista de Tabelas

# Lista de Figuras

# Capítulo 1

# Introdução

O presente relatório documenta o planeamento, conceção e desenvolvimento do projeto *Silent Protocol*. Este é um videojogo focado na furtividade, espionagem digital e resolução de puzzles, desenvolvido no âmbito da Unidade Curricular de Projeto da Licenciatura em Engenharia Informática e Multimédia do ISEL.

A motivação para a criação do *Silent Protocol* ultrapassa a componente de entretenimento, assumindo um caráter de consciencialização tecnológica. Diariamente, empresas e utilizadores particulares assumem que os seus dados confidenciais estão protegidos, ignorando frequentemente práticas básicas de segurança. O objetivo fundamental deste projeto consistiu em criar um ambiente imersivo que demonstrasse, de forma prática, a facilidade com que informações críticas podem ser roubadas caso não sejam devidamente protegidas.

Para transmitir esta mensagem sem descurar a componente de entretenimento, o jogo afasta-se do confronto direto comum na maioria dos videojogos modernos. A tensão decorre do risco constante de deteção. Assim, o jogador é obrigado a adotar um comportamento furtivo para contornar a Inteligência Artificial dos NPC, a recolher informação através de *hacking* de dispositivos vulneráveis, e a explorar falhas de arquitetura em sistemas de segurança como as redes de videovigilância. Os contributos técnicos deste trabalho incluem o sistema de rotas de NPC com rotinas dinâmicas distribuídas por cinco pisos, o sistema duplo de suspeita que separa a confiança da empresa (*company awareness*) da suspeita geral, e a integração de cinco minijogos que traduzem conceitos de cibersegurança para mecânicas de jogo.

# 1.1 Enquadramento Ético

Dado o tema sensível abordado em *Silent Protocol*, foi necessário definir uma fronteira clara entre a consciencialização em cibersegurança e o ensino de práticas ilícitas. A partilha de métodos reais de ataque informático, como a quebra de criptografia simétrica, a engenharia reversa ou a utilização detalhada de ferramentas de interceção ilegítima, levanta questões éticas e legais. Em Portugal, estas práticas são reguladas, entre outros diplomas, pela Lei do Cibercrime (Lei n.º 109/2009), que define como crime o acesso ilegítimo e a interceção ilegítima de dados.

Para respeitar os princípios éticos da Engenharia Informática e evitar que o jogo pudesse servir como meio de aprendizagem para ataques reais, optou-se por utilizar um processo de abstração nas mecânicas de *hacking*. Desta forma, a componente de intrusão digital do jogo é apenas ilustrativa. O jogador não necessita de conhecimentos técnicos reais para quebrar cifras. Em vez disso, tem de explorar o cenário para encontrar palavras-passe, credenciais ou comandos de terminal que foram deixados por outros funcionários. Ao introduzir esses comandos pré-definidos nos terminais do jogo, consegue avançar na narrativa e resolver os puzzles.

Esta abordagem garante que o projeto cumpre o seu objetivo de sensibilizar para os riscos das fugas de informação e da má gestão de acessos, sem ultrapassar os limites éticos ou incentivar práticas de cibercrime.

# 1.2 O Caso Real que Inspirou o Projeto

A narrativa de *Silent Protocol* e o seu enquadramento concetual têm por base dois incidentes reais de privacidade e exfiltração de dados ocorridos na última década, designadamente o caso da Cambridge Analytica e a monitorização de tráfego efetuada pela aplicação Onavo Protect.

**O Caso Cambridge Analytica.** Em 2013, através da aplicação de testes de personalidade "*This Is Your Digital Life*", o investigador Aleksandr Kogan recolheu dados de cerca de 270 mil utilizadores voluntários. Contudo, tirando partido de permissões da API Graph do Facebook, a recolha estendeu-se aos dados de todos os amigos desses utilizadores, sem o seu consentimento. O resultado foi a exfiltração de perfis de cerca de 87 milhões de utilizadores, cujos dados foram transferidos para a consultora política Cambridge Analytica para traçar perfis psicográficos e direcionar publicidade política segmentada em campanhas eleitorais. A exposição do caso em 2018 resultou em multas sem precedentes ao Facebook e na falência da consultora.

**O Caso da VPN Onavo.** Em 2013, o Facebook adquiriu a empresa israelita Onavo e disponibilizou a aplicação "Onavo Protect", promovida como uma ferramenta gratuita de segurança e privacidade (VPN). A aplicação foi descarregada por mais de 33 milhões de utilizadores. Contudo, ao canalizar o tráfego de rede dos dispositivos pelos seus servidores, a aplicação permitiu ao Facebook monitorizar de forma passiva a atividade dos utilizadores, identificando que aplicações concorrentes estavam a crescer. Estes dados serviram de inteligência concorrencial, influenciando decisões como a aquisição do WhatsApp em 2014 e o lançamento das Instagram Stories em 2016 para conter o Snapchat. A aplicação foi removida da App Store em 2018 por violação das políticas da Apple.

Estes dois casos revelam o padrão comum de dados que, tendo sido recolhidos sob um pretexto inofensivo ou legítimo, acabaram desviados e monetizados de forma oculta, constituindo este abuso de confiança o motor narrativo de *Silent Protocol*.

## Da Realidade para a Nexus Corp

A premissa do jogo traduz estes cenários para a empresa fictícia Nexus Corp. O jogador assume o papel de Alex Mercer, um jornalista infiltrado como júnior de cibersegurança, que dispõe de cinco dias, de segunda-feira até às 17h30 de sexta-feira, para expor o "Projeto Hélix", que consiste num esquema interno de exfiltração e venda de dados de 2,4 milhões de clientes.

Entre os 5 dias, o jogador terá que explorar os 5 pisos presentes na empressa (Receção, Executivo, Servidores, Apartamentos, CEO). O jogador terá acesso no ínicio do jogo acecsso livre à receção, ao piso Executivo e ao piso dos Apartamentos. Os restantes 2 pisos encontram-se bloqueados e só podem ser desbloqueados através de cartões de acesso, que por sua vez só são obtidos ao recolher intel específica. Essa intel está espalhada por todo o mundo do jogo, obrigando o jogador a explorar os diferentes espaços e interagir com o ambiente.

Os métodos e ferramentas do jogo estabelecem uma ligação direta a esta realidade. A presença de um analisador de pacotes de rede (inspirado no WireShark) no posto de trabalho do jogador e o facto de a informação mais sensível estar guardada no piso executivo/CEO refletem o facto de que os desvios de dados massivos dependem de decisões e cumplicidades ao mais alto nível das organizações, exigindo uma análise forense detalhada e progressiva para ligar todos os intervenientes da conspiração.

# Capítulo 2

# Trabalho Relacionado

O desenvolvimento deste projeto foi inspirado por três referências principais, cada uma contribuindo para uma mecânica específica no jogo, nomeadamente a recolha de informação sem ultrapassar a marca dos cinco dias, o sistema de vigilância no qual o jogador não poderá levantar suspeita e o sistema de atributos de personagem.

## Welcome to the Game

Welcome to the Game é um jogo de puzzle/horror em que o jogador navega na *dark web* através do computador da sua personagem enquanto procura por pistas espalhadas por vários sítios, enquanto tenta não ser rastreado pelo FBI. A referência relevante para Silent Protocol é o facto de ter o computador como interface de recolha de informação que o jogador tem de utilizar para progredir. Este princípio foi adaptado ao contexto de espionagem corporativa em vez de *dark web*, ou seja, o jogador recolhe informações através de e-mails internos, servidores e conversas em vez de pistas soltas em websites anónimos. A diferença fundamental é que, em Welcome to the Game, a ameaça (FBI) é externa, enquanto em Silent Protocol a ameaça é local e social, pois a suspeita vem dos NPC que partilham o espaço físico com o jogador, o que exige que a personagem mantenha uma aparência normal em simultâneo com a recolha de informação.

## Five Nights at Freddy's

Five Nights at Freddy's estrutura a experiência em cinco noites de dificuldade crescente e dá ao jogador acesso a câmaras de vigilância para monitorizar o espaço a partir de um posto fixo. Silent Protocol adota a estrutura de cinco dias de trabalho como mecanismo de progressão e para o aumento da dificuldade, juntamente com o acesso a câmaras de vigilância como forma de observação do ambiente. A diferença está na função da câmara, em Five Nights at Freddy's a câmara serve para deteção de ameaças e reação defensiva num espaço estático, enquanto em Silent Protocol serve para o planeamento de rotas de infiltração, para além do jogador não estar confinado a um posto fixo.

## Fallout

O sistema S.P.E.C.I.A.L. de Fallout atribui pontos a sete atributos (Força, Perceção, Resistência, Carisma, Inteligência, Agilidade, Sorte) que o jogador distribui na criação de personagem, influenciando opções de diálogo e resultados de mecânicas ao longo do jogo. Silent Protocol usa a mesma estrutura de sete atributos, com nomenclatura e definições correspondentes, para influenciar interações com NPC, o mundo e o desempenho em minijogos. Esta é a referência com menor grau de diferenciação face à fonte original. A estrutura de atributos foi adotada de forma direta e a distinção reside sobretudo na aplicação. Os atributos em Silent Protocol condicionam especificamente o sucesso em mecânicas de stealth social e minijogos, um domínio de aplicação que não existe no Fallout original.

# Capítulo 3

Este capítulo descreve o modelo definido para o Silent Protocol, um jogo de infiltração e puzzle em primeira pessoa que junta stealth social a minijogos inspirados em cibersegurança. Começa pelos requisitos identificados para o projeto, segue para os fundamentos concetuais e tecnológicos adotados e termina na abordagem seguida para os concretizar, ao longo de um edifício corporativo com cinco andares e de uma experiência de cinco dias de jogo.

# 3.1 Modelo Proposto

# Requisitos

**Requisitos funcionais.** O requisito central do projeto era permitir uma dupla vida dentro do edifício onde o jogador cumpre tarefas de trabalho legítimas enquanto recolhe e utiliza informação de forma discreta. Desse requisito central resultaram os seguintes pontos:

* Movimento em primeira pessoa e acesso progressivo aos cinco andares do edifício (receção, andar executivo, andar dos servidores, andar das suítes e piso do CEO).  
* Um ciclo de dia e noite, integrado numa experiência estruturada em cinco dias com um ritmo diário fixo, da chegada e atribuição de tarefas até à infiltração noturna e ao descanso, com a pressão a aumentar dia após dia.  
* Um sistema de tarefas diárias, apresentado através de um computador de trabalho com caixa de entrada e lista de tarefas.  
* Um sistema de suspeita, que sobe e desce consoante o comportamento do jogador e atravessa três limiares (atenção, investigação e expulsão).  
* Um sistema de sono e fadiga, com um mínimo de sete horas de sono por noite, sob pena de penalizações no dia seguinte.  
* Minijogos que traduzem, de forma acessível, conceitos de cibersegurança, tais como a captura e organização de pacotes, a descodificação, a desencriptação e a troca entre dispositivos.  
* Fontes de intel diversas, como câmaras de vigilância, dispositivos comprometidos, documentos físicos e portas protegidas por keypad ou cartão.  
* NPC com rotinas, diálogo e níveis de atenção próprios.  
* Um sistema de criação de personagem por distribuição de pontos em sete características (Força, Perceção, Resistência, Carisma, Intelecto, Agilidade e Sorte).  
* Três finais distintos (Denúncia, Extorsão e Lealdade), com vitória ao sobreviver aos cinco dias e alcançar um final, e derrota ao atingir a suspeita máxima ou ao ser apanhado numa área restrita durante a noite.

**Requisitos não funcionais.** O jogo tinha de correr de forma fluida em computadores modestos, o que levou à escolha de um mundo em baixo polígono e de cenários contidos. Tinha também de manter o jogador sempre informado do seu estado através de um HUD permanente e claro, com a barra de suspeita, a fadiga, a bateria da lanterna, a lista de tarefas e um indicador discreto de ruído durante a noite.

**Casos de utilização.** O jogador é o único ator do sistema e interage com o mundo através de quatro casos principais, nomeadamente cumprir tarefas de trabalho, explorar e observar o edifício, recolher intel e resolver os puzzles associados, e decidir como e quando usar a informação reunida. Os dois últimos casos foram tratados como prioritários, por serem o centro da proposta de valor do jogo, ficando os restantes numa posição de suporte a esse núcleo.

# Fundamentos

Do ponto de vista concetual, o *design* assenta em três pilares. O primeiro é a ideia de que a informação é poder, ou seja, toda a progressão do jogador vem de recolher, organizar e usar informação. O segundo é a dupla vida entre o trabalho legítimo e a infiltração. O terceiro é o risco calculado, em que qualquer ação mais poderosa aumenta a suspeita e tem de ser compensada por um comportamento mais cauteloso a seguir. É também este terceiro pilar que sustenta a premissa narrativa, pois o jogador entra no edifício com uma identidade falsa e vai descobrindo, ao longo dos dias, que o CEO e alguns clientes estão a negociar algo antiético, sem que essa descoberta possa ser feita de forma apressada.

Ao nível do género, o jogo apoia-se em mecânicas comuns aos jogos de infiltração e *stealth* social, onde o comportamento fora do normal num contexto de trabalho é tão arriscado como ser avistado fisicamente, e em minijogos temáticos que tornam processos de cibersegurança compreensíveis para jogadores sem formação técnica na área.

Do ponto de vista tecnológico, o jogo foi construído em Unity3D, com um mundo 3D *low-poly* para garantir desempenho em *hardware* modesto e clareza visual durante a exploração e o stealth. As interfaces, como os ecrãs de computador, o keypad e os menus, foram implementadas em Canvas 2D sobreposto ao mundo 3D, permitindo separar com clareza a camada de exploração da camada de puzzle.



# 3.2 Abordagem

A arquitetura do jogo organiza-se em torno de um conjunto de gestores (*managers*), cada um responsável por uma camada distinta do sistema. O GameManager mantém o estado global, o dia e a noite, o progresso e o final alcançado, e trata da gravação automática no fim de cada dia. O UIManager centraliza o HUD, os menus e os ecrãs de computador, além das mensagens de feedback e dos tutoriais contextuais. O PlayerController trata do movimento, da interação, do inventário, da lanterna e do ruído produzido pelo jogador, e regista também a suspeita gerada diretamente pelas suas ações. O NPCManager e o NPCcript tratam das rotinas, das patrulhas, das reações à suspeita e do diálogo com os NPC. O TaskManager gere o ciclo de tarefas diárias, e cada minijogo é gerido pelo seu próprio script dedicado (WireSharkManager, TerminalManager, etc.), validando o sucesso ou a falha de cada tentativa e atribuindo a recompensa correspondente.

O sistema de suspeita foi implementado como uma variável partilhada entre o PlayerController e o NPCManager, que sobe com ações como falhar tarefas, permanecer em áreas restritas, usar câmaras de forma excessiva ou ser visto a aceder a terminais fora do posto de trabalho, e desce com o cumprimento de tarefas e com comportamento social neutro dentro do horário normal. Ao atravessar os limiares de atenção e de investigação, os NPC e os guardas alteram as suas rotinas e aumentam as patrulhas, tornando o risco cada vez mais evidente em vez de terminar o jogo logo ao primeiro erro. O keypad segue esta mesma lógica, com um limite de tentativas ligado à suspeita, pelo que forçar um código repetidamente tem custo direto no resto do jogo.

A lanterna, usada durante a noite, ilustra o pilar do risco calculado pois ligar a lanterna melhora a visibilidade do jogador, mas aumenta a hipótese de ser notado por um guarda, uma vez que os NPC consideram a presença de luz como fonte de suspeita acrescida. A bateria drena em tempo real enquanto está ligada e recarrega automaticamente no início de cada nova noite. O ambiente sonoro acompanha esta alternância entre calma e risco, com sons de escritório e música discreta durante o dia e som posicional 3D mais tenso durante os puzzles e a infiltração noturna.

Ao aceder a um computador ou a uma câmara, a perspetiva muda da primeira pessoa para uma vista fixa dedicada à interface, o que reduz a confusão e concentra a atenção do jogador no puzzle em curso.

Os cinco andares foram desenhados com risco crescente e tipos de intel diferentes, de modo a que o jogador tenha sempre de escolher entre um ganho maior num andar mais arriscado ou um ganho menor e mais seguro num andar já conhecido. A título de exemplo, no andar dos servidores o jogador pode iniciar um minijogo de desencriptação num terminal; se for bem sucedido, obtém uma mensagem interna que liga o CEO a um cliente específico, disponível depois na tabela de intel do computador de trabalho para ser cruzada com uma credencial obtida de noite no andar das suítes. É o cruzamento destas pequenas peças de informação que permite ao jogador optar, no final, por denunciar, extorquir ou proteger a empresa.

Por fim, a criação de personagem funciona como uma camada adicional sobre este núcleo, de modo que a distribuição de pontos pelas sete características influencia, por exemplo, a facilidade de notar um pormenor num documento, de convencer um NPC numa conversa ou de manter o desempenho num dia mais longo e cansativo.

# 3.3 Edifício

O jogo divide-se em cinco pisos distintos.

- Receção;  
- Executivo;  
- Servidores;  
- Apartamentos;  
- CEO;

Cada piso tem utilidades diferentes para o jogador e contribuem de maneiras diferentes, mas conjuntas, para o decorrer do jogo.

# Receção

Este piso encontra-se aberto para todos os que queiram entrar, abrangendo tanto visitantes como trabalhadores. O jogador pode obter opiniões do público em relação à empresa e poderá tentar conversar com as rececionistas para obter informações valiosas que o ajudarão a vencer o jogo.

# Executivo

Neste piso, o jogador executa as suas tarefas diárias para simular o trabalho regular na empresa. É possível falar com outros colegas para obter informações interessantes, escutar reuniões ou telefonemas importantes entre os diferentes departamentos.

# Servidores

Este piso abriga grande parte dos dados estratégicos da empresa, dos trabalhadores e dos clientes. Por ser uma zona de alta segurança, o jogador não consegue aceder no primeiro dia, mas, após obter a devida credencial, poderá aceder aos computadores locais para tentar extrair os ficheiros necessários para atingir o final pretendido.

# Apartamentos

Este espaço destina-se ao descanso diário de todos os trabalhadores da empresa, incluindo o jogador. No seu quarto, é possível encontrar a lanterna útil para explorações noturnas, além do computador pessoal que concede acesso remoto aos pontos de vista das câmaras previamente desbloqueadas.

# CEO

Por fim, temos o piso do CEO. Este é o piso com mais segurança, pois é onde se encontra a pessoa com maior poder na empresa inteira e será ele a chave principal para o fim do jogo, ou seja, como interagimos com ele dado a quantidade de informação que tenhamos.

# 3.4 NPC

# Roles de NPC

Existem cinco tipos diferentes de NPC, designadamente os descritos a seguir.

- Colleague;  
- Boss;  
- Receptionist;  
- Guard;  
- Visitor.

Cada NPC tem utilidades diferentes no jogo, têm objetivos diferentes e o jogador é capaz de interagir com eles de várias maneira.  
O “Colleague” aparece apenas nos pisos do Servidor e Executivo, pois são eles que trabalham com o jogador no dia a dia na empresa. Navegam entre os dois pisos realizando as suas tarefas do dia a dia. O jogador pode falar com eles para obter ajuda em determinadas tarefas ou para conseguir obter informação valiosa que o poderá ajudar daí em diante.  
O “Boss” é o NPC que fica no piso do CEO, pois representa o maior estado de poder da empresa.  
A “Receptionist” são as rececionistas. Estas ficam nas secretárias e contêm informações valiosas sobre reuniões e horários de colegas que o jogador poderá tentar obter para ter mais informação sobre a empresa.  
O “Guard” patrulha pelos diferentes pisos à procura de qualquer aspeto que lhes salte à vista como suspeito, ou seja, se alguém não estiver a fazer o que é suposto. O jogador tem de evitar chamar-lhes à atenção, ou arrisca-se a perder o jogo.  
Por último, o “Visitor” tem por objetivo andar no piso da Receção e pode ser utilizado para o jogador conseguir obter uma visão exterior da empresa, ou seja, qual é a opinião pública da mesma.

# Sistema de Rotas

O sistema de rotas de NPC é um dos mais difíceis no jogo inteiro pois existem muitas variáveis que têm de ser tomadas em conta, muitas opções para poderem criar rotas, e muitos tipos diferentes de NPC. Cada um deles terá rotas diferentes dependendo das suas roles, alguns terão rotas fixas, outros terão rotas iniciais antes de começarem a fazer rotas ao acaso. Existem rotas que podem ser partilhadas independentemente das roles, NPC poderão escolher rotas alternativas com base num random, etc.  
Tudo isto foi considerado importante no desenvolvimento desta mecânica, pelo qual demorou vários dias a ficar completamente implementado.

# Sistema de Deteção

Existem dois sistemas diferentes de deteção, compostos pela mecânica de “company awareness” e pela de suspeita geral.  
A primeira funciona como uma base para avaliar as tarefas diárias concluídas pelo jogador e condiciona os possíveis finais de jogo de acordo com a confiança demonstrada no desempenho das funções atribuídas.  
A suspeita geral é mais direta, subindo quando o jogador realiza alguma ação suspeita sob o olhar de outro funcionário. Esta mecânica divide-se em três estados distintos:  
- Atenção
- Investigação  
- Expulsão

À medida que o jogador vai ficando suspeito vai passando por estes três estados. O primeiro avisa o jogador que o nível está a subir e que tem de ter cuidado, o segundo indica ao jogador que começou uma fase de investigação onde os guardas que estiverem por perto vão investigar o último sítio conhecido pelo jogador onde ele tenha subido a suspeita pela última vez, o último estado avisa o jogador que todos os guardas irão estar a persegui-lo e tentar expulsá-lo. Se tiverem sucesso, o jogador perde o jogo.  
Caso este pare de fazer a atividade que estava a fazer e volte para um local normal e permitido, o nível de suspeita vai descendo gradualmente de acordo com o tempo passado desde a última subida.

# 3.5 Intel

A recolha de informação (Intel) é o motor da jogabilidade de *Silent Protocol*. A informação está dispersa pelo edifício da Nexus Corp sob as formas de documentos físicos, *post-its*, e-mails, ficheiros em computadores de colegas e conversas com NPC. A captura de pacotes de rede também integra este ecossistema.

A exploração e correlação de pistas foram estruturadas com base em técnicas reais de segurança e investigação, traduzidas para mecânicas de jogo acessíveis:

* **OSINT (Open Source Intelligence)** consiste na recolha de informação estrategicamente relevante a partir de fontes públicas ou de livre acesso. Em *Silent Protocol*, o jogador aplica este princípio de forma passiva, visto que a maior parte da informação não exige invasão técnica de sistemas, mas sim a procura atenta por papéis esquecidos em secretárias, impressoras ou e-mails abertos em postos de trabalho desbloqueados.
* **Engenharia Social (Elicitação e Pretexting)** explora o fator humano para obter acessos. A própria posição do jogador como funcionário júnior é um exercício de *pretexting*, em que se recorre a uma identidade falsa para gerar confiança. Através do sistema de diálogos, o jogador interage com os NPC e usa o pretexto de ser um colega inexperiente ou refere pistas previamente encontradas para manipular os diálogos (elicitação) e extrair credenciais, rotinas e segredos.
* **Dumpster Diving** assenta na recuperação de dados sensíveis a partir de lixo físico ou digital. No jogo, manifesta-se ao explorar  os caixotes de lixo nos gabinetes e a reciclagem ("Trash Can") dos computadores dos NPC, onde informação confidencial apagada ou descartada por negligência pode ser recuperada.
* **Forense Digital** reflete-se na análise de caixas de correio eletrónico e na recuperação de e-mails eliminados que ainda se encontram em pastas de arquivo temporário, demonstrando que os dados digitais persistem no sistema mesmo após a sua eliminação aparente.
* **Efeito Dominó (Correlação)** baseia-se na premissa de que nenhuma prova ou credencial crítica está disponível de forma isolada. O jogo exige que o jogador correlacione os dados obtidos de múltiplas fontes, de forma que um e-mail intercetado pode fornecer a chave para interpretar um *post-it*, que por sua vez revela o código de acesso a um gabinete onde se encontra um cartão magnético. Esta estrutura em cadeia simula a realidade de auditorias de segurança e investigações reais.

# 3.6 Tarefas Diárias

Para o jogador conseguir infiltrar-se corretamente na empresa terá de fingir que está legitimamente a fazer o trabalho para o qual foi contratado. Existem quatro possíveis tarefas que podem ser feitas, com um mínimo de duas por dia, nomeadamente Imprimir Documento, Escrever Documento, Arquivar Documento e Entregar Documento que o jogador terá de concluir dentro de um intervalo de tempo específico para se conseguir enquadrar na empresa sem que ninguém desconfie dele. Cada tarefa surge e desaparece de acordo com um horário próprio, definido dia a dia, o que permite variar a ordem e a distribuição das tarefas ao longo da jornada de trabalho. Caso o jogador não consiga realizar a tarefa a tempo, ou deixe passar o tempo limite até esta ficar indisponível, a métrica "Company Awareness" irá aumentar. Esta métrica define o quão a empresa confia no jogador para fazer o seu trabalho e influenciará a capacidade de escolher cenários diferentes de fim de jogo. Estas tarefas do dia a dia organizam-se ao longo do horário de trabalho para manter o jogador ocupado e não lhe dar demasiado tempo livre para explorar, incentivando o bom uso do tempo que tem disponível. 

# Escrever Documento

Esta tarefa realiza-se no computador do jogador, no piso Executivo, e simula o trabalho administrativo do dia a dia. O jogador tem de preencher um documento com várias lacunas, escolhendo entre um conjunto de opções fixas para cada uma delas. As escolhas feitas não têm uma resposta "errada" do ponto de vista da tarefa em si, o que importa é que todas as lacunas fiquem preenchidas, mas influenciam pesos narrativos que vão condicionar, mais tarde, para que final o jogador está a encaminhar-se. Se o jogador submeter o documento com lacunas por preencher, a tarefa é considerada mal feita e a suspeita geral da empresa sobre o jogador sobe.

# Arquivar Documento

Depois de ter um documento no inventário, o jogador tem de o entregar no arquivo correto, correspondente ao departamento a que esse documento pertence. Existem três arquivos físicos no escritório, um por departamento, e cabe ao jogador deduzir, através da exploração e das pistas que vai reunindo, qual é o correto. A informação não lhe é dada diretamente. Arquivar no sítio certo mantém a confiança da empresa; arquivar no sítio errado é visto como um erro claro de trabalho, o que faz subir tanto a suspeita geral como o Company Awareness, já que o documento acaba por chegar a quem não devia.

# Imprimir Documento

Esta tarefa obriga o jogador a interagir com uma das impressoras disponíveis no piso Executivo, escolhida aleatoriamente. Primeiro, tem de se deslocar ao seu computador de trabalho para que depois possa imprimir o documento. Ao interagir com a impressora, esta vai imprimir um documento físico que o jogador pode então apanhar. Só é possível transportar um documento de cada vez, o que obriga o jogador a gerir bem a ordem pela qual completa as suas tarefas.

# Entregar Documento

Ao contrário de Arquivar Documento, que trata do processo interno de arquivo, Entregar Documento representa a necessidade de fazer chegar um documento diretamente a um colega específico de um departamento, em vez de o depositar num arquivo físico. Esta tarefa reforça a ideia de que o jogador está inserido na rotina normal da empresa, sendo visto a cumprir pedidos e prazos como qualquer outro trabalhador. Tal como as restantes tarefas, entregar fora do prazo ou ao destinatário errado tem impacto negativo na confiança que a empresa deposita no jogador.



# 3.7 Mini Jogos

O *Silent Protocol* conta com cinco mini-jogos, cada um inspirado num conceito ou técnica real do mundo da segurança informática e da investigação corporativa. Os cinco são a captura e análise de pacotes de rede (baseada no Wireshark), a descodificação e desencriptação de mensagens cifras (baseadas nos algoritmos DES e AES), a acesso não autorizado e a tomada de controlo de câmaras de vigilância, a escuta de reuniões e a interceção de telefonemas internos.

Todos eles partilham a mesma filosofia de *design*. Cada puzzle tem raízes num processo real. O objetivo não é ensinar segurança informática ao jogador, mas fazer com que ele sinta que está genuinamente a investigar dentro de um sistema que poderia existir numa empresa real.

# Mini Jogo 1 \- Acesso a Câmaras de Vigilância

Um dos vários puzzles que foram incorporados no jogo é o minijogo de ganhar acesso às câmaras de vigilância. Ao interagir com uma, o jogador irá encontrar uma interface na qual terá de coordenar o sinal da frequência da câmara com o seu próprio sinal de modo a conseguir dar “override” à segurança da mesma, deste modo o jogador consegue ter acesso ao ponto de vista da câmara através do seu computador pessoal, localizado no seu quarto. Caso não consiga, terá de tentar novamente uma vez que tiver a oportunidade, pois ao falhar o “hack” o jogador ganha muita suspeita fazendo com que os NPC investiguem o que se passou, que poderá levar à expulsão e a possível perda do jogo.

# Mini Jogo 2 \- Escuta de Reunião

A mecânica de escuta de reunião responde ao problema concreto de *design* sobre como recolher informação verbal (não escrita, como e-mails ou documentos) sem participar diretamente na conversa. A solução adotada trata a reunião como um fluxo de texto que se desenrola em tempo real, linha a linha, simulando a progressão natural de uma conversa ouvida à distância.

Nem todas as linhas da reunião contêm informação relevante. As que contêm estão marcadas como capturáveis e o jogador só consegue extrair essa informação premindo o botão de captura durante uma janela temporal limitada. O tempo de exibição da linha mais um período de tolerância imediatamente a seguir. Fora dessa janela, a oportunidade é perdida de forma permanente para aquela instância da reunião.

Esta janela de captura obriga o jogador a manter atenção contínua ao longo de toda a reunião, já que não há forma de saber antecipadamente que linha vai conter a *keyword*. Reforça diretamente o pilar de *design* "informação é poder", pois a recompensa (intel) só é obtida através de atenção ativa, não de presença passiva.

A mecânica inclui ainda um custo associado à simples permanência na zona da reunião antes de iniciar a escuta, pelo que a suspeita acumula de forma passiva enquanto o jogador está próximo mas ainda não decidiu interagir. Esta escolha de *design* impede que o jogador espere indefinidamente por um momento "seguro" para começar a escutar, ou seja, cria uma decisão de risco calculado. Quanto mais tempo o jogador demora a comprometer-se com a ação, maior o custo de suspeita acumulado antes mesmo de a informação ser obtida.

# Mini Jogo 3 \- Escuta de Telefonemas

A escuta de telefonemas parte do mesmo princípio da escuta de reunião. Captura de informação através de uma janela temporal ligada a uma *keyword*, mas introduz uma camada adicional de dificuldade, caracterizada pela possibilidade de múltiplos canais de chamada ativos em simultâneo, até um máximo de três, dos quais o jogador só pode monitorizar um de cada vez.

Esta é a diferença concetual central face à mecânica anterior. Na escuta de reunião existe apenas uma fonte de informação e o desafio é temporal (acertar o momento). Na escuta de telefonemas o desafio passa a ser também de atenção dividida, uma vez que os canais não selecionados continuam a decorrer em segundo plano, e se uma *keyword* surgir num canal que o jogador não está a monitorizar nesse momento, essa informação é automaticamente perdida, sem hipótese de recuperação. O jogador é obrigado a decidir, em tempo real, que canal vale mais a pena acompanhar, sabendo à partida que está a sacrificar informação dos restantes.

Existe também uma penalização assimétrica para o erro, visto que capturar fora de uma janela ativa é ativamente punido, gerando suspeita instantânea e terminando a interceção de imediato, o que obriga o jogador a recomeçar do zero se quiser tentar outra chamada. Isto distingue-se da escuta de reunião, onde falhar uma captura tem custo zero além de perder aquela informação específica; aqui, uma tentativa de captura mal calculada tem custo direto sobre o estado global de suspeita.

Por fim, a disponibilidade da mecânica está limitada a uma janela horária específica dentro do dia de trabalho, o que a liga ao ciclo diário do jogo em vez de estar disponível a qualquer momento. Isto reforça que a recolha de informação tem de ser planeada em torno do horário e não é uma ação sempre disponível a pedido.

# Mini Jogo 4 \- Captura de Pacotes de Rede

A mecânica de captura de pacotes de rede no *Silent Protocol* assenta em conceitos reais de redes de computadores, adaptados para o contexto de espionagem do jogo.

**Pacotes e Protocolos.** Numa rede de computadores, os dados não viajam como um bloco único, mas sim divididos em pequenos fragmentos chamados pacotes. Cada pacote viaja de forma independente e contém um *header* (com metadados de controlo como IPs de origem e destino) e um *payload* (o conteúdo real da mensagem). Para garantir a comunicação entre computadores heterogéneos, este tráfego segue regras estritas chamadas protocolos.
**A Vulnerabilidade do HTTP.** O HTTP (*Hypertext Transfer Protocol*) é o protocolo que serve de base à web. O aspeto de interesse pedagógico para o jogo reside no facto de o HTTP original transmitir os dados em texto simples (*cleartext*), o que permite a qualquer nó intermédio com acesso à rede local ler o conteúdo completo das mensagens. Embora sistemas modernos utilizem HTTPS (encriptando os dados via TLS), redes internas antigas ou mal configuradas podem manter tráfego em texto simples vulnerável.

**O Wireshark e o Modo Promíscuo.** O Wireshark é a ferramenta real utilizada por administradores e auditores de segurança para capturar e analisar tráfego. O seu funcionamento baseia-se em colocar a placa de rede em modo promíscuo (*promiscuous mode*). Se em condições normais uma placa apenas processa pacotes endereçados ao seu computador, no modo promíscuo ela passa a capturar e a expor todo o tráfego físico que passa pela interface de rede, independentemente do destinatário.

**Ataques Man-in-the-Middle no Jogo.** No *Silent Protocol*, o jogador explora esta vulnerabilidade através de uma simulação de ataque *Man-in-the-Middle* (MITM). A partir do computador de trabalho, o jogador monitoriza o tráfego que flui na rede interna da Nexus Corp. O jogo reflete o comportamento real de tráfego misto, visto que algumas mensagens capturadas chegam legíveis (emulando HTTP vulnerável), ao passo que outras surgem cifradas (emulando encriptação robusta), exigindo o cruzamento com chaves obtidas no cenário para permitir a sua leitura no terminal. 

**DES e a fragilidade histórica.** O DES (*Data Encryption Standard*) foi adotado em 1976 e utiliza uma chave de 56 bits. O seu aspeto mais relevante reside no facto de ilustrar a evolução computacional, considerando que, embora fosse seguro nos anos 70, em 1999 a organização EFF provou ser possível quebrar a sua chave por força bruta em menos de 23 horas, tornando-o obsoleto para sistemas modernos.

**AES e a robustez do Rijndael.** Para suceder ao DES, foi selecionado em 2001 o algoritmo Rijndael (AES, sigla para *Advanced Encryption Standard*), criado por dois criptógrafos belgas. Operando com chaves de 128 a 256 bits, a sua segurança é tão elevada que uma chave de 128 bits apresenta 2^128 combinações possíveis, exigindo mais tempo do que a idade do universo para ser decifrada por força bruta com a tecnologia atual.

**Hashes como impressões digitais.** O jogo simula também o uso de *hashes* criptográficos, como MD5 ou SHA-256. Ao contrário da encriptação, um *hash* é uma função matemática de sentido único e irreversível que gera uma assinatura de tamanho fixo para qualquer conjunto de dados, servindo sobretudo para verificar a integridade de ficheiros ou armazenar palavras-passe de forma segura.

**A Chave como Elo Mais Fraco no Jogo.** No *Silent Protocol*, o jogador interceta mensagens encriptadas com AES ou DES e precisa de encontrar a chave no cenário através de conversas, documentos físicos ou computadores alheios. Esta mecânica demonstra o princípio real de que a segurança de um sistema depende inteiramente do segredo da chave e não do algoritmo, pois a encriptação AES mais robusta torna-se inútil se a chave for escrita num *post-it* colado ao monitor do utilizador.

# 3.8 Design e Identidade Visual

O *design* do *Silent Protocol* foi concebido para contrastar a rotina corporativa diurna e a infiltração sob constante vigilância durante a noite, recorrendo a escolhas cromáticas e mecânicas de interface que reforçam a imersão na temática de cibersegurança e espionagem.

**Assets e Modelos Tridimensionais.** Para a construção dos cenários, foram selecionados modelos modulares de mobília de escritório, permitindo povoar de forma diferenciada cada um dos pisos da Nexus Corp (Receção, Executivo e Servidores). Os modelos das personagens não jogáveis (colegas, guardas, rececionista, visitantes e o CEO) e as respetivas animações de locomoção e agachamento foram recolhidos para garantir uma representação humana credível, ajudando o jogador a identificar visualmente o papel de cada personagem à distância.

**Iluminação Dinâmica e Ciclo Dia-Noite.** A atmosfera visual é definida pela transição contínua de iluminação ao longo do dia de jogo. A variação de luz amarelada matinal para tons alaranjados de tarde e azul-escuro noturno permite ao jogador compreender organicamente a passagem do tempo e planear as suas ações de infiltração. A transição de luz dita também o comportamento do ambiente, em que a noite traz uma redução drástica da visibilidade e exige a utilização da lanterna.

**Indicadores Visuais de Alerta (HUD de Suspeita).** A perceção de perigo é transmitida ao jogador através de um elemento de interface dinâmico representado por um olho estilizado. O diâmetro do globo ocular e da íris reage em tempo real à proximidade dos guardas e a atos ilícitos, expandindo-se para sinalizar o perigo de deteção. Quando a suspeita é elevada, o elemento de interface dinâmico pulsa para simular a tensão e o pânico da personagem. O HUD muda também de cor consoante o estado de alerta (cinzento em segurança, laranja sob atenção, vermelho-alaranjado sob investigação ativa e vermelho brilhante no estado de expulsão).

**Interface de Terminal e Computador.** Para as aplicações utilitárias e minijogos que decorrem no computador de trabalho, optou-se por uma estética de terminal de comandos retro. A paleta de cores foca-se em tons de verde sob fundo escuro, simulando monitores de fósforo verde clássicos. Adicionalmente, os elementos mecânicos do computador, tais como o temporizador radial de sono ou o teclado numérico com luzes indicadoras de aceitação, foram desenhados para ter pouca densidade de informação no ecrã, reduzindo o ruído visual e mantendo o foco do jogador.

# Capítulo 4

# 4.1 Implementação da Identidade Visual e Atmosfera

A concretização técnica da identidade visual e dos indicadores de jogo foi desenvolvida no Unity através de scripts dedicados que interagem com o sistema de iluminação global e com a interface do utilizador.

## Iluminação Dinâmica e Rotação Solar

A transição atmosférica é controlada pelo script `DayNightLightController.cs`, que calcula a hora do dia através do relógio central do `TimeManager`. O comportamento assenta nas seguintes parametrizações:
* **Cor e Ambiente.** O script avalia a hora normalizada (entre 0 e 1) e atualiza a cor de uma luz direcional e a cor ambiente global (`RenderSettings.ambientLight`) com base nos gradientes `lightColorGradient` e `ambientColorGradient`.
* **Intensidade.** A intensidade luminosa segue a curva `intensityCurve`, apresentando um valor de 0.0 durante a noite, subindo para 0.40 às 07:40, atingindo o pico de 1.20 às 12:00 e descendo para 0.85 ao final da tarde.
* **Elevação.** O ângulo do Sol é determinado pela curva `sunElevationCurve` (entre -30 graus na meia-noite e 65 graus ao meio-dia), mantendo o azimute em 170 graus.

## HUD Dinâmico de Suspeita

O indicador de stealth é gerido pelo script `SuspicionHUD.cs` e implementa as seguintes regras visuais:
* **Escala do Olho.** O tamanho do globo ocular (`eyeWhiteRect`) e da íris (`irisRect`) acompanha o rácio de suspeita suavizado por `Mathf.SmoothDamp`. O rácio é mapeado através de `Mathf.SmoothStep` para permitir que o olho cresça até 2.2 vezes e a íris até 1.5 vezes o seu tamanho original.
* **Efeito de Pulsação.** Quando o rácio é superior a 0.60, é iniciada uma oscilação sinusoidal (`Mathf.Sin(Time.time * 2f) * 0.03f`) adicionada à escala do olho.
* **Código de Cores.** As cores do HUD alteram-se com base no estado do `SuspicionManager`, correspondendo a cinzento (`colorNone`), laranja (`colorAttention`), vermelho-laranja (`colorInvestigation`) e vermelho com pulsação intermitente (`colorExpulsion`). A opacidade do painel (`CanvasGroup.alpha`) varia de forma linear entre 0.25 e 1.0.

## Estilo e Comportamento do Computador

As interfaces dos computadores de secretária utilizam o script `TerminalUI.cs` e seguem regras estritas de apresentação de dados:
* **Paleta de Cores CRT.** A separação de conteúdos baseia-se em cores estáticas de fósforo verde, utilizando `ColSys` (verde escuro) para delimitadores, `ColPrompt` (amarelo) para avisos, `ColInput` (verde brilhante) para comandos e `ColPlain` (verde claro) para texto desencriptado.
* **Mecânicas de Interface.** O ecrã de sono (`UIManager.cs`) traduz o tempo em frações de rotação da imagem radial (`fillAmount`), enquanto as tentativas no teclado numérico usam corrotinas (`WrongCodeDelay` e `CorrectCodeDelay`) para bloquear os botões temporariamente e atualizar LEDs visuais.


# 4.2 Mini Jogos

# Mini Jogo 1 \- Acesso a Câmaras de Vigilância

Este minijogo é inspirado em como os fones anti ruído funcionam.  
Para os fones cancelarem o som exterior, impedindo que o mesmo entre no ouvido da pessoa, precisam de replicar o sinal do som exterior de maneira inversa de modo a cancelá-lo. Assim, após ter sido estudada a teoria por detrás desse sistema, foi implementada, de maneira semelhante, neste minijogo de ganhar acesso às câmaras de vigilância. Para o fazer, o jogador terá de replicar o sinal espelhado e inverso da mesma, resultando numa semelhança de como os fones anti ruído funcionam na vida real.

## Identificação

Os ficheiros de código que implementam este minijogo são “[CameraHackInteractable.cs](http://CameraHackInteractable.cs)”, “[CameraHackPuzzle.cs](http://CameraHackPuzzle.cs)”, “[CameraSystem.cs](http://CameraSystem.cs)”, “CameraTerminal.cs” , “[CameraViewUI.cs](http://CameraViewUI.cs)” e “[SurveillanceCamera.cs](http://SurveillanceCamera.cs)”.

## Estrutura de dados

A estrutura deste minijogo é centrada na utilização de apenas scripts. O objetivo dos mesmos residem apenas em guardar o estado de quantas câmaras já foram desbloqueadas, mostrar os seus pontos de vista quando o jogador lhes aceder através do seu computador pessoal e interagir com elas para tentar resolver os puzzles que enfrentam ao jogador, aumentando a sua dificuldade com base no número de câmaras já desbloqueadas pelo mesmo. Por estes motivos, não é necessário utilizar ScriptableObjects, mas apenas render textures para mostrar os pontos de vista, juntamente com os scripts.

## Deteção de início

Para dar início ao minijogo, o jogador terá de se aproximar à câmara dentro de uma certa distância que é calculada através de um raycast feito pelo script da câmara do jogador com uma distância de seis.. Após chegar perto da mesma, o jogador terá de apontar a mira na sua direção. Consequentemente, a câmara ficará com um efeito que mostra que pode ser interagida. Após o jogador clicar na tecla E para interagir, irá aparecer uma interface que mostra o conteúdo do minijogo, dando começo à mecânica.  
Para o jogador poder observar os pontos de vista que desbloqueou ao realizar os minijogos de outras câmaras, o mesmo terá de se movimentar ao seu computador pessoal. Uma vez que se aproxime o suficiente e que aponte a mira de modo que o computador fique a brilhar, o jogador terá de premir a tecla E, a partir desse momento ele poderá interagir trocando entre os diferentes pontos de vista.

## Lógica central em execução

Assim que a interface de desbloquear a câmara é ligada, o minijogo aparece. Assim que a interface é ligada, é definido em primeiro lugar a dificuldade do mesmo, atingindo um nível máximo de 8\. Para a interface aparecer corretamente, terá de primeiro gerar a sequência de sinais originais referentes à câmara. Essa sequência é composta por barras com tamanhos diferentes, criando um sinal discreto com base nas suas alturas. Posteriormente, o sinal do lado do jogador é criado ao lado do original, inicialilzando todas as barras a metade da sua altura máxima. Para alternar entre as diferentes barras, o jogador utilizará as teclas A e D, e para aumentar/diminuir o tamanho das mesmas terá de utilizar as teclas W e S. É utilizado o método Update() para o código correr a sua lógica, especialmente para verificar se o jogador conseguiu, a certo ponto, igualar o seu sinal ao sinal espelhado e inverso da câmara. À medida que o nível ficar mais alto, a dificuldade aumenta através de fazer com que haja menos tempo disponível para o jogador terminar o minijogo e através de fazer “tremer” o sinal do jogador, ou seja, fazendo com que as suas barras fiquem mais altas ou baixas a partir do seu estado final definido pelo jogador. Assim, o sinal poderá não ficar concluído caso essas variações ligeiras nas alturas faça com que o intervalo máximo de distância que é permitido ao jogador falhar. No fim, caso o jogador ganhe, perca ou cancele o minijogo, é utilizada uma corrotina para mostrar a mudança na interface para o jogador poder entender o que foi concluído.  
No que é referente ao seu computador pessoal para mostrar os diferentes pontos de vista, o jogador terá acesso apenas às câmaras desbloqueadas, pois as que ainda não concluiu o minijogo apenas será mostrado um ecrã de restrição.

## Ligação a outros sistemas

Este sistema das câmaras interliga-se ao sistema de suspeita no jogo. Caso o jogador conclua incorretamente o minijogo, a suspeita geral sobre o jogador é aumentada substancialmente, chegando no mínimo ao estado de investigação e, no máximo, ao estado de expulsão. Adicionalmente, para proibir o jogador de alternar entre as câmaras que já desbloqueou, foi implementado que, a cada troca, a suspeita geral aumenta e a interface do computador fique gradualmente mais vermelha e com barulho estático visual.  
O computador pessoal que mostra as diferentes câmaras só poderá ser acedido quando é de noite, para tal verificados o estado do tempo local de jogo através do script [TimeManager.cs](http://TimeManager.cs). Para mostrar as interfaces ao jogador utilizados o script [UIManager.cs](http://UIManager.cs) que é o que trata de todas as interfaces presentes no jogo, juntamente com toda a informação visual que poderá aparecer ao jogador.

## Imagem

## 

# Mini Jogo 2 \- Escuta de Reunião

Este minijogo tem como objetivo o jogador conseguir obter informações com base no que é falado na reunião, sem que os participantes, ou outros NPC, reparem que ele está a tentar ouvir aquilo que não deve. Desta modo, o jogador consegue aprender novos dados que lhe poderá ser útil ao decidir o cenário final do jogo.

## Identificação

O ficheiro de código que implementa este minijogo é apenas o “MeatingEavesdropScript[.cs](http://CameraHackInteractable.cs)”. Devido à natureza simples deste minijogo, consegue ser tudo compreendido num ficheiro apenas de código.

## Estrutura de dados

A estrutura deste minijogo é centrada na utilização única de um script. Com base neste ficheiro foi possível guardar as falas dos NPC da reunião, bem como aquelas que transportam intel importante para o jogador. Adicionalmente, também possui a interface que é mostrada ao jogador. Uma vez que é necessário incluir as linhas de diálogo, foi decidido usar uma classe serializable para guardá-las e indicar se contêm intel, intel esta que é representada através de um ScriptableObject “IntelItem”. Este ScriptableObject inclui informações relevantes como o título da intel, conteúdo e a localização onde foi capturada.

## Deteção de início

Para dar início a este minijogo, o jogador terá de se aproximar da porta da sala de reuniões. Uma vez que chegue perto da mesma, esta começará a brilhar indicando ao utilizador que pode interagir com ela, mostrando que pode começar a escutar a reunião. Esta aproximação terá que ser menor que três e é verificada através de uma distância calculada pela diferença dos vetores3D do jogador e da porta da sala. Porém, para o jogador poder escutar a reunião, terá de o fazer enquanto a mesma está a decorrer, portanto, para alcançar este objetivo foi utilizado eventos que indicam que a reunião começou. Este ficheiro de código encontra-se subscrito a esse evento e receberá essa notificação quando a mesma começar.

## Lógica central em execução

A lógica reside numa corrotina. Primeiramente, o jogador terá de clicar na tecla E (verificado pelo método Update) para interagir com a porta da sala de reuniões enquanto a mesma está a decorrer a uma distância menor que três. Posteriormente, é começada a corrotina que dá início ao minijogo, esta mostrará todas as linhas de diálogo dos NPC, quer tenham intel ou não. À medida que os NPC vão conversando, aparece uma animação que escrita a cada caractere, como escrever manualmente. Quando uma linha de diálogo tiver intel, um botão na interface do minijogo ficará com que seja possível interagir com o mesmo. O jogador terá, então, uma pequena janela de tempo no qual poderá capturar essa informação.

## Ligação a outros sistemas

Caso a intel consiga ser capturada, é feita a ligação ao ficheiro “[IntelInventory.cs](http://IntelInventory.cs)” para guardar no inventário de intel do jogador a informação que acaba de ser capturada. Caso o jogador esteja perto da porta enquanto uma reunião esteja a decorrer, é adicionada suspeita passivamente como castigo enquanto o tempo passa, o que desincentiva o jogador a tentar escutar a reunião. Se o jogador não conseguir guardar a intel e perca a oportunidade não será castigado ativamente com o aumento de suspeita. Por fim, é feita a conexão com o script “[UIManager.cs](http://UIManager.cs)” para mostrar a interface do minijogo e comunicar com as suas componentes visuais para alterar textos ou botões.

## Imagem

# Mini Jogo 3 \- Escuta de Telefonemas

Este minijogo é semelhante ao de escutar a reunião, porém noutro contexto. Aqui, o jogador irá escutar vários telefonemas que estão a ser feitos em tempo real, sendo que só pode escutar um ao mesmo tempo, fazendo com que tenha de escolher aquele que pense que lhe dará intel, senão perde a chance da recolher e não a poder usar para decidir o cenário final do jogo.

## Identificação

Os ficheiros de código que implementa, este minijogo são o “[PhoneCallData.cs](http://PhoneCallData.cs)”, “[PhoneCallUI.cs](http://PhoneCallUI.cs)” e “[PhoneInterceptScript.cs](http://PhoneInterceptScript.cs)”. Dado que este minijogo é mais complexo, foram utilizados mais do que um ficheiro de código face ao minijogo de escutar a reunião. O ficheiro “[PhoneCallData.cs](http://PhoneCallData.cs)” é um ScriptableObject que tem por objetivo criar os conteúdos dos telefonemas e juntar os dados importantes, caso hajam.

## Estrutura de dados

Este minijogo utiliza maioritariamente o ficheiro de código “[PhoneCallUI.cs](http://PhoneCallUI.cs)” que irá mostrar ao jogador a inteface que terá de utilizar para trocar entre telefonemas e obter a intel, quando esta se apresentar disponível. É utilizado o ScriptableObject “[PhoneCallData.cs](http://PhoneCallData.cs)” para guardar informações relevantes como as linhas de diálogo e os scriptableObjects “IntelItem”. Desta maneira, é conseguida organizar eficientemente um diálogo coerente entre vários telefonas diferentes, podendo conter, ou não, intel guardada por detrás.

## Deteção de início

Para o jogador conseguir detetar que pode começar este minijogo, terá de se aproximar do telefone que contém o ficheiro de código “[PhoneInterceptScript.cs](http://PhoneInterceptScript.cs)”, pois é este que indica que telefone pode ser, ou não, alvo de ser intercetado. Para tal ocorrer, o jogador também terá de chegar num tempo em que o telefonema esteja a ocorrer. Quando o jogador se aproximar do telefone, este começará a brilhar para indicar que pode interagir com ele. Caso não chegue no intervalo de tempo em que o telefonema esteja a ocorrer, não lhe é mostrada a interface do minijogo. Este ficheiro de código encontra-se subscrito a esse evento e receberá essa notificação quando a mesma começar.

## Lógica central em execução

A lógica de código para correr o minijogo está presente num método Update que verifica o canal de chamada em que o jogador escolheu escutar (perante os diferentes telefonemas que ocorrem ao mesmo tempo) e numa corrotina que dará início a toda a lógica do minijogo. Para dar início a tudo, o jogador terá de premir na tecla E apontando para o telefone em questão, que será quando a corrotina é inicializada .  
Primeiro é chamada uma função que começa por inicializar as estruturas dos diferentes canais de telefonemas, incluindo os diálogos, as informações que o jogador poderá capturar e se os canais já terminaram os telefonemas.  
Assim que os canais estiverem prontos, é começada uma corrotina para cada canal. Dentro de cada uma, começarão a serem mostrados os diálogos através de uma animação semelhante à do minijogo de escutar a reunião, onde os caracteres vão aparecendo um a um, lentamente. Quando uma linha de diálogo contiver um pedaço de intel, é aberta uma janela de tempo no qual é avisado ao jogador que pode capturar a informação. O botão que o permite fazer é, no entanto, sempre possível ser clicado. Quando passar o tempo de capturar os dados, a janela fecha-se e, se o jogador tentar clicar no botão, ganha suspeita imediata por falhar o minijogo.

## Ligação a outros sistemas

Semelhante ao minijogo de escutar a reunião, para dar início ao minijogo, o jogador terá de chegar num tempo em que o telefonema esteja a ocorrer. Esse tempo é medido e verificado através do ficheiro de código “[TimeManagar.cs](http://TimeManagar.cs)” que tem por objetivo medir o tempo local dentro do jogo.  
Ao contrário do minijogo de escutar uma reunião, o jogador aqui poderá clicar no botão de capturar intel mesmo que esta não exista no momento. Ou seja, caso o jogador clique no botão de capturar intel, e o diálogo que estiver a ser passado nesse momento não contenha IntelData, então a suspeita geral perante o jogador será aumentada imediatamente, para além de já ser aumentada assim que o minijogo comece para desincentivar o jogador a tentar escutar o telefonema.   
Para aumentar a suspeita, é necessário comunicar com os métodos presentes no ficheiro de código “[SuspicionManager.cs](http://SuspicionManager.cs)”. Caso a intel seja corretamente capturada, é feita a conexão ao ficheiro “[IntelInventory.cs](http://IntelInventory.cs)” para armazenar no inventário de intel do jogador a informação capturada. Caso o jogador não consiga recolher a intel, perde a oportunidade para o resto do jogo, mas não será castigado ativamente com o aumento de suspeita.   
Por último, é chamado o script “[UIManager.cs](http://UIManager.cs)” para mostrar ao jogador a interface do minijogo e comunicar com as suas componentes visuais para alterar textos ou botões.

## Imagem

# Mini Jogo 4 \- Captura de Pacotes de Rede

No capítulo de abordagem foi descrito o conceito real por detrás deste mini-jogo. A captura passiva de tráfego de rede, a distinção entre pacotes em texto simples e pacotes encriptados, e a mecânica de Man-in-the-Middle enquanto fonte de informação dentro do mundo da Nexus Corp. 

Este mini-jogo apresenta-se no computador pessoal do jogador. Uma app no ambiente de trabalho. Quando a abrimos encontramos uma UI onde, do lado direito, são apresentados os pacotes que chegam enquanto a app está ligada e do lado esquerdo um histórico, que mostram os pacotes que chegam enquanto a app não está ligada.

## Identificação

Este mini-jogo é composto por quatro scripts principais que residem no mesmo GameObject: “WiresharkManager.cs”, “WiresharkUI.cs”, “[PacketGenerator.cs](http://PacketGenerator.cs)” e o modelo de dados “[PacketData.cs](http://PacketData.cs)”. A estes juntam-se componentes auxiliares referenciados ao longo do capítulo: “CryptoHelper.cs”, “NetworkSchedule.cs”, “PacketRowUI.cs”, “HistoryRowUI.cs” e “[GameClipboard.cs](http://GameClipboard.cs)”.

O fluxo de dados é unidirecional: o “[PacketGenerator.cs](http://PacketGenerator.cs)” produz pacotes e passa-os ao “[wiresharkManager.cs](http://wiresharkManager.cs)”, este mantém o estado e instrui o “[wireSharkUI.cs](http://wireSharkUI.cs)” que nunca escreve estado, apenas apresenta o que recebe e invoca os métodos públicos do “[WiresharkMabager.cs](http://WiresharkMabager.cs)” em resposta a eventos de UI. Tem também um evento de input, um botão de copiar, que guarda numa memória o payload do pacote para mais tarde, no mini-jogo da desencriptação, colar.

## Estrutura de dados

O “[PacketData.cs](http://PacketData.cs)” é uma classe simples marcada com \[Serializable\], o que permite que o Unity a serialize para inspeção e que seja usada em ScriptableObjects e listas expostas no Inspector.

Cada instância representa um único pacote de rede capturado, com os seguintes campos:

(fazer tabela com campo \- tipo \-Descricao)

| PacketId | string | Identificador sequencial (ex: PKT-0041) gerado pelo PacketGenerator |
| :---- | :---- | :---- |
| ConversationId | string | Agrupa pacotes da mesma troca (ex: CONV-CEO-ADLER) |
| SrcIP / DstIP | string | Endereços IP fictícios de origem e destino |
| Protocol | string | "TCP" ou "UDP" |
| EncryptionType | string | "AES", "DES" ou "NONE" |
| EncryptedPayload | string | Conteúdo cifrado (ou texto simples se NONE) |
| PlainText | string | Texto original — usado internamente; não exposto na UI sem chave |
| Hash | string | Hash gerado pelo CryptoHelper |
| MessageIndex | int | Posição do pacote dentro da conversa |
| IsImportant | bool | Marca pacotes com intel narrativa relevante |
| Timestamp | float | Time.time no momento de criação |

A distinção entre o “[EncryptedPayload](http://EncrytedPayload.cs)” e “[PlainText](http://PlainText.cs)” é centrada para a mecânica. O jogador vê texto ilegível (hex cifrado) nos pacotes encriptados e texto simples nos que têm EncryptionType \== "NONE". O “[PlainText](http://PlainText.cs)” permanece no objeto em memória, o que permite que o sistema de desencriptação (mini-jogo 5\) o revele quando o jogador encontrar a chave correta, sem necessitar de uma segunda passagem pela lógica criptográfica.

## Lógica central em execução

O conteúdo narrativo do mini-jogo (quais os pacotes que aparecem, a que horas, entre que endereços IP, com que tipo de encriptação…) é definido fora do código, em assets do tipo “[NetworkSchedule.cs](http://NetworkSchedule.cs)”. Estes são ScriptableObjects configurados no Inspetor do Unity, um por dia de jogo.

O “[WiresharkManager.cs](http://WiresharkManager.cs)” mantém dois estados, a lista de pacotes ao vivo e o dicionário de histórico indexado por ConversationID. Expõe quatro métodos públicos que os restantes componentes invocam diretamente.

O “ReceivePacket” insere o pacote no topo da lista ao vivo e dá ao “WiresharkUI a criação da linha correspondente no scroll. SetHistory é chamado pelo PacketGenerator sempre que o histórico muda (no arranque do dia e cada vez que um pacote chega com a app fechada) e propaga a atualização para o painel de histórico na UI. 

## 

## Ligação a outros sistemas

O mini-jogo integra-se com três sistemas transversais. O TimeManager fornece a hora atual do jogo para que o PacketGenerator saiba quando disparar cada pacote e quais enviar diretamente para o histórico. O SuspicionManager recebe incrementos pontuais quando o jogador acede ao histórico de conversas alheias, ou seja, cada vez que o jogador acede ao histórico a suspeita geral pode aumentar. O GameClipboard permite transportar payloads inscritos para este mini-jogo para o mini-jogo de desencriptação criando um fluxo de trabalho coerente entre os dois sistemas.

# Mini Jogo 5 \- Descodificação e Desencriptação: 

No capítulo anterior (abordagens) foi explicada a teoria por detrás deste mini-jogo. A distinção entre criptografia simétrica e assimétrica, o funcionamento do DES e do AES como cifradores de blocos e o papel dos hashes criptográficos como “impressões digitais” de dados. Esta parte do capítulo descreve como esses conceitos foram traduzidos para código dentro do Unity, e as simplificações que tiveram de ser feitas para que a teoria se tornasse uma mecânica de jogo jogável.

## Identificação

Este mini-jogo é composto por dois scripts principais que residem no mesmo GameObject, o terminal do computador do jogador. “[TerminalManager.cs](http://TerminalManager.cs)” e “[TerminalUI.cs](http://TerminalUI.cs)”. A estes junta-se um componente estático auxiliar “[CryptoHelper.cs](http://CryptoHelper.cs)”, que concentra toda a lógica criptográfica, e uma ligação direta ao “GameClipboard.cs” , o componente que transporta os pacotes capturados no Mini-Jogo 4 para dentro desse terminal.

O fluxo de dados segue a mesma separação de responsabilidade já vista no mini-jogo anterior, o “[TerminalUI.cs](http://TerminalUI.cs)” nunca decide nada por si próprio, apenas apresenta linhas de texto coloridas no ecrã e encaminha os eventos de input (Enter, botão Colar) para o “[TerminlaManager.cs](http://TerminlaManager.cs)”. É este último que mantém todo o estado do processo de desencriptação e decide o que deve ser escrito no terminal a seguir. O “[CryptoHelper.cs](http://CryptoHelper.cs)” é invocado pelo “[TerminalManager.cs](http://TerminalManager.cs)” sempre que é necessário encriptar, desencriptar, converter hexadecimal ou gerar um hash.

A teoria descreve o DES e o AES como cifradores de blocos simétricos, ou seja, a mesma chave serve para encriptar e desencriptar, e a segurança depende interinamente do sigilo dessa chave. Esta ideia é o que estrutura toda a mecânica do “[CryptoHelper.cs](http://CryptoHelper.cs)” 

Em vez de simular criptografia com uma função fictícia, optou-se por usar as implementações reais da biblioteca *System.Security.Cryptography* do .NET, com chave e vetores de inicialização fixos, definidos como constantes internas do jogo

Lógica central em execução 

O “[TerminalManager.cs](http://TerminalManager.cs)” mantém uma máquina de estados (TerminalState), que mostra o processo de desencriptação real.

Quando o jogador cola um pacote, o estado passa a Pasted e a UI mostra o conteúdo cifrado como hexadecimal. O jogador terá que usar os comandos .aes ou .des que correspondem à tentativa do jogador de “adivinhar” o algoritmo correto, vai ser o equivalente a tentar a chave certa. Se o tipo de encriptação for certo, o jogo valida diretamente, caso contrário o método “*TryDecryptPayload*” tenta mesmo a desencriptação real e considera sucesso apenas se o resultado for texto interpretável. Fazendo uma comparação com o que acontece na vida real. Mesmo sem saber previamente qual o algoritmo certo, uma tentativa com a chave errada resulta em dados ilegíveis. 

Só depois de uma desencriptação bem-sucedida (Decrypted), aparece um texto em hexadecimal e o jogador terá que usar o comando .hexdecode, para converter para texto simples e completando o paralelo ponderado com o processo real de desencriptação. 

Ligação a outros sistemas 

O mini-jogo integra-se diretamente com o GameClipboard, o mesmo sistema descrito no capítulo do Mini Jogo 4\. É este componente que transporta o  EncryptedPayload, o PacketId e o EncryptionType de um pacote capturado mini-jogo anterior para dentro do terminal de desencriptação que fecha o ciclo entre os dois mini-jogos. Primeiro capturar o tráfego, depois parti-lo. Esta ligação é o que permite ao jogo cumprir, a nível de fluxo de jogabilidade, a ideia central da teoria. Que a informação interceptada só se torna útil depois de ultrapassada a camada de encriptação.

# 4.3 Intel

# Recolha de Informação \- OSINT, Engenharia Social e Forense Digital

No capítulo anterior (Abordagem) foi explicada a teoria real por detrás da mecânica de recolha de informação do *Silent Protocol.* O conceito de OSINT como recolha passiva de fontes abertas, as técnicas físicas e digitais de engenharia social (dumpster diving, elicitação pretexting), os princípios de forense digital sobre dados “apagados” mas recuperáveis, e o princípio da correlação como efeito dominó. Este capítulo descreve como estes conceitos foram traduzidos para código dentro do Unity.

## Identificação

Este sistema é composto por vários scripts distribuídos por dois subsistemas que partilham a mesma lógica de fundo, a recolha de intel física ( documentos, post-its, objetos no mundo) e a recolha de intel digital (e-mails e lixo dos PCs dos NPC).

Do lado físico, “[IntelItem.cs](http://IntelItem.cs)” é o modelo de dados (ScriptableObjects) que representa uma peça de informação e “[IntelPickup.cs](http://IntelPickup.cs)” é o componente colocado nos objetos inteligíveis do mundo (os documentos, post-its…). Do lado digital “[EmailItem.cs](http://EmailItem.cs)” é o modelo de dados de um e-mail, e “[PCEmailManager.cs](http://PCEmailManager.cs)” e “[PCTrashManager.cs](http://PCTrashManager.cs)” gerem, respetivamente, a caixa de entrada e o lixo de um PC específico. A camada de apresentação é feito pelo “[IntelReadUI.cs](http://IntelReadUI.cs)” (o painel de leitura ao interagir com um objeto físico) e “[EmailUI.cs](http://EmailUI.cs)” (a aplicação de e-mail dentro do PC). Por fim, “[IntelInventory.cs](http://IntelInventory.cs)” funciona como o repositório centro, um “dossiê” do jogador, que recebe informações vindas de qualquer uma destas fontes.

O fluxo de dados replica o padrão já estabelecido nos mini-jogos anteriores: os managers (PCEmailManager, PCTrashManager) mantêm o estado e disparam eventos (OnEmailRecebido, OnItemRecebido), a UI (EmailUI, IntelReadUI)  reage a esses eventos e nunca decide nada por si, a decisão final de “guardar” uma peça de informação é sempre centralizada no jogador. O jogador é que decide se quer ou não guardar a informação no inventário. 

Lógica central em execução 

A ideia central da teoria (que o OSINT não exige acesso técnico, apenas saber onde procurar) é o que rege o comportamento dos objetos de intel espalhadas pelo mundo. Cada peça de informação física existe no cenário desde o início, mas permanece invisível até ao dia de jogo em que deve ser descoberta, tornando-se visível assim que esse dia chega. O jogador não “desbloqueia” nada tecnicamente, basta aproximar-se e interagir para que o conteúdo lhe seja mostrado, exatamente como a teoria descreve o caráter passivo do OSINT, em que o desafio está na descoberta e não no acesso. Enquanto o jogador lê essa informação, a sua detecção pelos NPC é temporariamente suspensa, reforçando a ideia de que consultar um documento não é, em si, um ato suspeito ou intrusivo.

Do lado digital, a mesma lógica de entrega é aplicada aos e-mails e ao lixo de cada PC, a informação não está disponível de imediato, mas vai sendo entregue ao longo do dia de jogo, tal como os pacotes de rede no mini-jogo do Wireshark. O que distingue o plano digital é a forma como trata a eliminação de dados. Quando o jogador ou NPC “apaga” um e-mail, este não desaparece, move-se apenas para o lixo, permanecendo acessível e restaurável, exatamente como a teoria explica que a eliminação de um ficheiro real apenas marca o espaço como disponível, sem destruir de imediato o conteúdo. 

Os e-mails em si funcionam também como uma fonte dupla de informação, por um lado o conteúdo da mensagem, por outro os seus metadados (quem enviou, a quem e quando). Esta separação permite que o jogador construa uma perceção de relações entre personagens mesmo a partir de e-mails cujo conteúdo pareça trivial, refletindo a ideia da teoria de que os metadados de comunicação revelam padrões e hierarquias tanto ou mais do que o próprio texto.

Por fim, o princípio da correlação de informação (de que nenhuma peça isolada tem grande valor, mas o cruzamento entre várias sim) é o que estrutura o acesso à informação mais sensível do jogo. Certos e-mails críticos permanecem encriptados até o jogador reunir um conjunto específico de fragmentos de informação, recolhidos em locais e momentos diferentes, físicos ou digitais. Só quando todos os fragmentos exigidos por essa mensagem em concreto já foram recolhidos é que o conteúdo se torna legível. Este mecanismo é a tradução direta do “efeito dominó” descrito anteriormente. Uma descoberta isolada não revela nada por si só, é a acumulação e o cruzamento de pistas dispersas que abre o caminho seguinte, replicando a forma como investigações reais avançam por passos incrementais.

Ligação a outros sistemas

Toda a informação recolhida pelo jogador, seja ela encontrada fisicamente no mundo ou dentro de um e-mail, converge sempre para o mesmo dossiê central do jogador, independente da sua origem. É esta centralização que permite ao sistema de correlação funcionar de forma transversal, um fragmento encontrado numa secretária pode perfeitamente ser a peça que falta para desencriptar um e-mail encontrado num PC completamente diferente, sem que exista qualquer ligação direta programada entre os dois locais, a ligação existe apenas ao nível da informação recolhida pelo jogador.

O sistema depende ainda da progressão temporal do jogo para decidir quando a informação surge ou se revela, à semelhança do que acontece no mini-jogo de captura de pacotes de rede, em que o mesmo relógio do jogo dita a entrega de conteúdo. 

# 4.4 Tarefas Diárias

# Escrever Documento

Esta tarefa desenrola-se no computador pessoal do jogador, no piso Executivo, e simula o preenchimento de um documento de trabalho do dia a dia. Ao contrário dos minijogos, que testam a destreza ou a atenção do jogador sob pressão de tempo, esta tarefa foi pensada para ser rápida e sem fricção mecânica, servindo sobretudo para reforçar a rotina de escritório e para recolher, de forma discreta, as escolhas do jogador que vão condicionar mais tarde o final do jogo.

## Identificação

Os ficheiros de código que implementam esta tarefa são “WriteDocumentUI.cs” e o ScriptableObject “DocumentTaskData.cs”, este último partilhado também pelas restantes tarefas relacionadas com documentos (Arquivar Documento e Entregar Documento), visto ser ele que guarda toda a informação referente a um documento específico.

## Estrutura de dados

O ScriptableObject “DocumentTaskData.cs” guarda o título do documento, o corpo de texto com lacunas marcadas por placeholders numerados (“{0}”, “{1}”, “{2}”, etc.) e um array de “BlankSlot”, uma classe serializable que contém, para cada lacuna, a resposta correta, as opções erradas, os pesos narrativos (“weightDenuncia”, “weightExtorsao” e “weightLealdade”) que vão influenciar o final do jogo, e ainda um peso de impacto no Company Awareness que só é aplicado mais tarde, quando o documento é arquivado. Este mesmo ScriptableObject guarda também o departamento correto e o destinatário correto do documento, campos que só interessam às outras duas tarefas. Já o ficheiro “WriteDocumentUI.cs” guarda apenas o estado local de execução, abrangendo os arrays “chosenWords” e “filledSlots” indexados de acordo com as lacunas do documento, o índice da lacuna ativa no momento e um conjunto fixo de quatro botões de escolha reutilizados entre lacunas.

## Deteção de início

Esta tarefa é despoletada através do método “OnEnable”, chamado sempre que o jogador abre o computador, em vez do habitual “Start”, precisamente para que o estado da task seja sempre verificado no momento certo em que o painel fica visível, e não com dados desatualizados de quando o objeto foi criado. É verificado se existe uma task ativa de manhã ou de tarde chamada “Escrever documento”; caso exista, mostra-se o painel do documento e chama-se “OpenDocument” com o documento do dia obtido através do “DocumentManager”; caso contrário, mostra-se um painel vazio a indicar que não há nenhuma tarefa deste tipo para fazer.

## Lógica central em execução

Ao abrir o documento, são inicializados os arrays “chosenWords” e “filledSlots” e o índice da lacuna ativa a zero, sendo depois reconstruído o texto do documento e mostradas as opções da primeira lacuna. O texto do documento é reconstruído de raiz sempre que há uma escolha nova, através de “RefreshBodyText”, em que as lacunas já preenchidas aparecem a branco e sublinhadas, a lacuna ativa no momento aparece a amarelo e sublinhada, e as restantes lacunas por preencher aparecem apenas como um traço simples, dando ao jogador uma perceção constante do seu progresso. Para cada lacuna, “ShowOptionsForCurrentBlank” organiza a resposta correta juntamente com as opções erradas nos quatro botões fixos, baralhando a sua ordem para que a resposta certa nunca fique sempre na mesma posição. Ao escolher uma palavra, “OnWordChosen” guarda a escolha, marca a lacuna como preenchida e avança para a próxima lacuna por preencher, saltando as que já têm resposta, voltando a mostrar as opções seguintes ou escondendo os botões assim que todas as lacunas estejam preenchidas. Ao submeter o documento, “OnSubmit” percorre todas as lacunas de forma que, se alguma ficar por preencher, a tarefa é considerada mal feita; independentemente do resultado, todas as escolhas feitas pelo jogador são guardadas através de “DocumentManager.SaveChoice”, pois os pesos narrativos de cada escolha contam sempre para o final do jogo, mesmo quando a tarefa em si é bem-sucedida.

## Ligação a outros sistemas

Esta tarefa liga-se ao “TaskManager” tanto para verificar se existe uma tarefa ativa, como para a marcar como concluída ou mal feita no momento da submissão. Liga-se também ao “DocumentManager” para obter o documento do dia e para guardar, de forma persistente, as escolhas do jogador, escolhas essas que serão consultadas mais tarde, de forma independente deste script, para determinar para que final o jogador se está a encaminhar. Por fim, ao submeter o documento, o painel é desativado, devolvendo o controlo à interface normal do computador.

## Imagem

# Arquivar Documento

Depois de ter um documento na mão, resultante da tarefa Imprimir Documento, o jogador tem de o entregar no arquivo físico correspondente ao departamento a que esse documento realmente pertence. Ao contrário de Escrever Documento, esta tarefa não testa reflexos nem exige rapidez, mas sim a capacidade do jogador de interpretar as pistas espalhadas pelo escritório e de deduzir, por si, qual dos três arquivos é o correto, pois essa informação nunca lhe é dada diretamente.

## Identificação

O ficheiro de código que implementa esta tarefa é “ArchiveScript.cs”, que estende a classe base “InteractableObject”, comum a todos os objetos interagíveis do jogo.

## Estrutura de dados

A estrutura assenta num enum “DepartmentType”, com os três departamentos possíveis do escritório (Recursos Humanos, Financeiro e Operações). Cada um dos três arquivos físicos existentes na cena é configurado, através de um campo serializado, com um destes departamentos, representando fisicamente um arquivo diferente. A validação da tarefa é feita por comparação direta com o campo “correctDepartment” guardado no “DocumentTaskData” que o jogador transporta consigo.

## Deteção de início

O brilho que indica que o arquivo pode ser interagido é controlado por “CheckShouldGlowByDefault”, que verifica apenas se o jogador tem, de facto, um documento na mão. Desta forma, o arquivo só convida à interação quando existe algo para arquivar, evitando indicações visuais enganadoras. Tal como nos restantes objetos interagíveis do jogo, o jogador aproxima-se e prime a tecla E para interagir.

## Lógica central em execução

Ao interagir, “Interact” verifica primeiro se o jogador não tem nenhum documento na mão, caso em que apenas dá feedback e sai sem mais nenhum efeito. Havendo documento, é comparado o seu departamento correto com o departamento deste arquivo específico para determinar se a escolha está certa. É depois chamado o “DocumentManager” para registar o arquivamento, aplicar os pesos narrativos e atualizar o Company Awareness, e o “TaskManager” para marcar a tarefa como concluída, correta ou não, sendo a penalidade por errar aplicada à parte, através da suspeita. O documento é removido da mão do jogador independentemente do resultado. Caso o departamento esteja errado, é imediatamente aumentada a suspeita geral através do “SuspicionManager”, com um nível de 1.5, um valor escolhido propositadamente por representar um erro claro de trabalho, mas não catastrófico, situando-se entre os níveis mínimo e máximo que a mecânica de suspeita permite.

## Ligação a outros sistemas

Esta tarefa liga-se ao “DocumentManager” para aplicar os pesos narrativos e atualizar o Company Awareness, ao “TaskManager” para a conclusão da tarefa, e ao “SuspicionManager” para penalizar de imediato um arquivamento incorreto. Liga-se ainda ao “PlayerController”, que guarda a referência ao documento que o jogador transporta e que é partilhada entre as tarefas de Imprimir, Arquivar e Entregar Documento, obrigando o jogador a gerir bem a ordem pela qual realiza as suas tarefas, já que só pode transportar um documento de cada vez.

## Imagem

# Entregar Documento

Ao contrário de Arquivar Documento, que trata do processo interno de arquivo, Entregar Documento obriga o jogador a encontrar e a entregar o documento em mãos a um colega específico de um departamento, sem que o jogo lhe indique diretamente quem é o destinatário correto. Esta tarefa reutiliza o mesmo script responsável pelo diálogo normal com as personagens nao jogáveis, distinguindo, consoante o contexto, se o jogador quer apenas conversar ou entregar um documento.

## Identificação

O ficheiro de código que implementa esta tarefa é “NPCcript.cs”, mais especificamente a sua lógica de “Interact” e o método privado “TryDeliverDocument”, em conjunto com o campo “correctRecipientID” guardado no “DocumentTaskData.cs”.

## Estrutura de dados

Cada NPC do jogo possui um campo privado serializado, “npcID”, um identificador único que é comparado com o “correctRecipientID” guardado no documento que o jogador transporta. Este campo tem de ser configurado manualmente no Inspector, por NPC, de forma a corresponder ao destinatário correto definido em cada documento do dia.

## Deteção de início

A deteção de início desta tarefa está integrada no método “Interact” já existente para o diálogo normal dos NPC, pelo que, se o jogador tiver um documento na mão e a tarefa “Entregar documento” estiver ativa no momento, a interação é desviada para “TryDeliverDocument” em vez de abrir o diálogo habitual. Desta forma, os NPC continuam a funcionar normalmente como parceiros de diálogo nos dias em que esta tarefa não está ativa, ou sempre que o jogador não tenha nenhum documento consigo.

## Lógica central em execução

Em “TryDeliverDocument”, a entrega é considerada correta apenas se o “npcID” deste NPC não estiver vazio e corresponder ao “correctRecipientID” do documento. O “TaskManager” marca a tarefa como concluída independentemente do resultado, sendo a penalidade por um destinatário errado aplicada à parte, tal como acontece em Arquivar Documento. O documento é removido da mão do jogador em qualquer um dos casos. Caso o destinatário esteja errado, é aumentada de imediato a suspeita geral através do “SuspicionManager”, reutilizando deliberadamente o mesmo nível (1.5) e a mesma origem “DocumentMisfiled” já usada em Arquivar Documento, uma vez que entregar a pessoa errada é, concetualmente, o mesmo tipo de erro do que arquivar no departamento errado, ainda que a mecânica em si (um NPC em vez de um arquivo físico) seja diferente.

## Ligação a outros sistemas

Esta tarefa liga-se ao “TaskManager” para verificar e concluir a tarefa, ao “SuspicionManager” para penalizar uma entrega incorreta, e ao “PlayerController” para aceder e limpar a referência ao documento transportado. Liga-se ainda ao sistema de diálogo normal dos NPC, já que a decisão entre conversar ou entregar um documento passa pelo mesmo ponto de entrada no código, preservando o funcionamento normal do diálogo nos dias sem esta tarefa ativa.

## Imagem Placeholder

# 4.5 NPC

## Non Playable Characters

Os NPC (Non Playable Characters) são vitais para o jogo. Por causa deles o jogador não pode entrar em locais que não deve quando alguém o estiver a observar, é graças a alguns que o jogador poderá obter informações importantes ao dialogar com eles e, mais importante, é devido a eles que existe a condição de perder o jogo através da expulsão do jogador.  
Para o desenvolvimento destas personagens foram usados os seguintes ficheiros de código: “[NPCcript.cs](http://NPCcript.cs)”, “[NPCpawner.cs](http://NPCpawner.cs)”, “[NPCManager.cs](http://NPCManager.cs)”, “[PatrolRoute.cs](http://PatrolRoute.cs)” e, para dialogar com eles, “[NPCDialogueData.cs](http://NPCDialogueData.cs)” (sendo este um ScriptableObject), “[DialogueTopic.cs](http://DialogueTopic.cs)” e “[DialogueCutscene.cs](http://DialogueCutscene.cs)”.

Queriamos que os NPC tivessem a possiblidade de falar com o jogador, mas também que tivessem a possibilidade de andar livremente pelos diferentes pisos do edificio, para além de apenas vaguear sem objetivo. Então, estabelecemos rotas para cada tipo de NPC, pois alguns são diferentes e portanto não faria sentido todos terem os mesmos percursos. Para dar manage disto foi criado o script PatrolRoute. Para atribuir a lógica principal deles fizémos o NPCcript que trata de interagir com os NPC, animá-los, começarem rotas, verificar suspeitas, etc. Existe um evento que faz com que haja uma reunião durante a tarde, mas apenas os NPC mais importantes do piso executivo fazem parte dela, e como precisávamos que os NPC apanhassem uma rota ao acaso bem como tratar de outras coisas gerais aos mesmos foi criado o script NPCManager. Para dar alguma utilidade dentro do jogo ao elevador para eles, fizémos o NPCpawner que trata de instanciar NPC de vez em quando dentro de um certo limite em casa piso.  
Para dialogarmos com eles temos os restantes scripts que tratam de preparmos linhas de diálogo com ScriptableObjects NPCDialogueData que são usadas para iniciar o diálogo com o jogaodr, quer seja geral ou com suspeita (caso o jogador a tenha subido). Para conversas mais especificas foi feito o código DIalogueTopic que também é um ScriptableObject e permite criar várias conversas com assuntos diferentes que os NPC podem dialogar com oj ogador. O ficheiro DialogueCutscene é mais simples pois a sua unica utilidade é fazer com que os NPC falem em voz alta para si mesmos. Por fim, para dar manage disto tudo foi feito o script DIalogueManager que trata de apresentar o diálogo inicial na primeira parte da interação, seguido de mostrar tópicos disponíveis de acordo com a suspeita ou nivel de carisma do jogador. Aós o jogador escolher na interface uma das opções de diálogo, este script processa as consequencias.  
Portanto, para podermos ter tudo conectado com os NPC, fazemos conexão com outros ficheiros de código, tais como [SuspicionManager.cs](http://SuspicionManager.cs), [TimeManager.cs](http://TimeManager.cs), [TaskManager.cs](http://TaskManager.cs) e [Gameevent.cs](http://Gameevent.cs).

É importante referir que existem varias possibilidades de rotas para cada NPC.  
o NPCManager é o real goat aqui,. Primeiro lugar temos um objeto muita grande no inspetor que nos permite criar objetos para pisos diferentes  e depois rotas para cada piso. então existem um monte de rotas para cada piso.. Depois para o evento da reunião guardamos os NPC que la vao e as rotas de cada um. Cada rota que está dentro daquele objeto bue grande pode ser partilhada por todos os NPC, dependendo do que está feito no inspetor do patrol route dona da rota, pois é aí que definimos quem é que pode correr essa rota, ou seja, que tipo de NPC, para além da probabilidade da fazer para não ser sempre igual e introduzir alguma imprevisibilidade no jogo, também incluimos se fazemos o NPC correr a rota em loop, se volta ao waypoint principal que iniciou a rota e se é uma rota de descansar, apenas para dar uma volta. Também incluimos um ID para o departamento de executivo. Todas estas variáveis sao muito importantes para o código. Para o NPCpawner temos variáveis do genero assigned route que é a rota que o NPC vai ficar caso seja atribuida especificamente para ficar pre definida, a start route que é a primeira rota a ser feita pelo NPC e quanto tempo é que o spawner pode volta a spawnar de acordo com a quantidade de instancias atualmente a funcionar por esse mesmo spawner.

## Guarda

O guarda patrulha os diferentes pisos do edifício e é o único tipo de NPC que reage ativamente ao ruído durante a noite. Possui uma corrotina dedicada (NoiseCheckRoutine) que verifica, a cada 0,2 segundos, se o jogador se encontra dentro do seu raio de audição (hearingRadius) e se está em movimento. Se o jogador for ouvido, o guarda entra em estado Investigate e desloca-se para a última posição conhecida do jogador. O ruído produzido pelo jogador varia conforme o modo de locomoção: correr gera um raio de 10 metros, andar gera 5 metros e agachar gera 2 metros. Quando a suspeita global atinge o limiar de Investigação, apenas os guardas entram no estado Investigate, enquanto os restantes NPC permanecem em Atenção. No estado de Expulsão, todos os guardas entram em Chase e perseguem diretamente o jogador. O NPCManager garante que apenas um guarda pode estar em rota de descanso em simultâneo, através do método CanGuardRest, o que assegura que há sempre cobertura mínima em todos os pisos.

## Visitante

O visitante circula exclusivamente no piso da Receção. É instanciado pelo NPCpawner, que controla a frequência de aparecimento e o número máximo de instâncias ativas por spawner. As rotas que o visitante pode percorrer são definidas no Inspector do PatrolRoute, com probabilidade associada a cada uma, introduzindo imprevisibilidade no fluxo de pessoas no piso de entrada. O visitante pode ser abordado pelo jogador através do sistema de diálogo. As suas linhas de diálogo (NPCDialogueData) oferecem uma perspetiva externa sobre a empresa, fornecendo ao jogador opiniões públicas sobre a Nexus Corp que podem conter pistas indiretas.

## Rececionista

A rececionista tem a flag isPatrolling a false, o que faz com que permaneça em estado Idle na sua secretária quando não está ativamente em rota. O campo isAtHome regista se a rececionista se encontra na sua posição fixa (homeBase), condição verificada pelo NPCManager antes de permitir que ela saia para outra rota. A rececionista tem um departmentID atribuído no Inspector, o que permite ao sistema de tarefas (Entregar Documento) validá-la como potencial destinatária de documentos. Ao dialogar com a rececionista, o jogador pode obter informações sobre horários de reuniões e rotinas de colegas, dados que facilitam o planeamento da infiltração noturna.

## Colega

O colega opera nos pisos Executivo e Servidores, navegando entre ambos através das rotas definidas no NPCManager. Cada colega tem um departmentID atribuído e pode ter uma assignedRoute fixa (um caminho pré-definido, como porta-secretária-elevador) ou obter rotas aleatórias do conjunto disponível para o seu tipo e piso. Alguns colegas recebem também uma startRoute, percorrida uma única vez ao aparecer, antes de entrarem no ciclo normal de patrulha. O método ForceRoute permite ao NPCManager redirecionar os colegas do piso Executivo para o cubículo de reuniões durante o evento de reunião da tarde. No sistema de diálogo, os colegas podem revelar tópicos (DialogueTopic) que variam conforme o nível de suspeita e o carisma do jogador, podendo fornecer informação importante caso o jogador escolha a abordagem correta.

## CEO

O CEO (Boss no código) reside no piso do CEO e representa o topo da hierarquia da empresa. A sua interação com o jogador é condicionada pela quantidade de intel recolhida ao longo dos cinco dias e pela métrica de Company Awareness. O CEO participa no evento de reunião através do método ForceRoute, tal como os colegas, e as suas linhas de diálogo (NPCDialogueData) contêm informação de nível narrativo elevado, diretamente relacionada com o Projeto Hélix. No final do jogo, a forma como o jogador interage com o CEO determina qual dos três finais (Denúncia, Extorsão ou Lealdade) é atingido, com base nas provas reunidas e nas escolhas feitas.



# Capítulo 5

# Validação e Testes

Por fazer.

## 5.1 Testes X

Por fazer.

## 5.2 Testes Y

Por fazer.

# Capítulo 6

# Conclusões e Trabalho Futuro

## 6.1 Conclusões

O projeto Silent Protocol cumpriu os objetivos definidos no início do semestre. Foi produzido um protótipo funcional que integra mecânicas de furtividade social, recolha de informação, minijogos inspirados em cibersegurança e um sistema narrativo com três finais distintos, tudo enquadrado num edifício corporativo de cinco pisos com NPC dotados de rotinas dinâmicas.

Os contributos técnicos centrais do projeto são o sistema de rotas de NPC, com suporte para rotas fixas, aleatórias e forçadas por evento, distribuídas por cinco pisos e condicionadas por tipo de NPC; o sistema duplo de suspeita, que separa a confiança operacional da empresa (Company Awareness) da suspeita comportamental geral, com três limiares que alteram progressivamente o comportamento dos NPC; e a integração de cinco minijogos que traduzem conceitos de cibersegurança (captura de pacotes, desencriptação simétrica, controlo de câmaras, escuta de reuniões e interceção de chamadas) para mecânicas de jogo acessíveis.

Do ponto de vista pedagógico, o projeto demonstrou que conceitos de cibersegurança podem ser integrados em mecânicas de jogo sem exigir conhecimentos técnicos prévios do jogador. As sessões de testes de jogabilidade e usabilidade confirmaram que X.

Todas as mecânicas previstas no *Game Design Document* foram implementadas e integradas no protótipo final. O ciclo completo de jogo, desde a criação de personagem até à obtenção de um dos três finais, está funcional e jogável.

## 6.2 Trabalho Futuro

Embora o protótipo esteja completo e funcional, existem áreas que beneficiariam de desenvolvimento adicional:

X

# Apêndice A

# Gestão de Versões

O controlo de versões do projeto foi feito exclusivamente com Git, utilizando um repositório alojado no GitHub. O desenvolvimento decorreu num ramo único (main), com *commits* incrementais que documentam a evolução do projeto desde a configuração inicial do projeto Unity até à versão final entregue.

O ficheiro .gitignore segue o template padrão para projetos Unity, excluindo diretórios como Library/, Temp/, Logs/, Builds/ e ficheiros de configuração de IDE (.vs/, .vscode/), garantindo que apenas o código-fonte, os *assets* e os ficheiros de configuração do projeto são versionados.

O repositório contém um total de 113 commits no ramo main. Não foram utilizados *feature branches* durante o desenvolvimento. A decisão de manter um ramo único deveu-se à dimensão reduzida da equipa (dois elementos), o que permitiu coordenar o trabalho sem necessidade de ramificação formal. Os *commits* seguem uma convenção descritiva em inglês, identificando a funcionalidade alterada ou adicionada em cada iteração.

# Apêndice B

# Estrutura de Entrega e Organização do Repositório

O repositório do projeto encontra-se organizado de forma a separar o código-fonte, os elementos artísticos, as configurações e a documentação de suporte, estando a estrutura principal dividida nas pastas e nos ficheiros descritos a seguir.

## Diretório Raiz


## Pasta Assets (Recursos do Jogo)

O diretório de trabalho no Unity está estruturado para organizar os recursos do jogo através das pastas descritas a seguir.


# Bibliografia

\[1\] Wikipedia, "Facebook-Cambridge Analytica data scandal," *Wikipedia, The Free Encyclopedia*. \[Online\]. Disponível em: https://en.wikipedia.org/wiki/Facebook-Cambridge_Analytica_data_scandal \[Acedido: Jul. 2026\]

\[2\] Wikipedia, "Cambridge Analytica," *Wikipedia, The Free Encyclopedia*. \[Online\]. Disponível em: https://en.wikipedia.org/wiki/Cambridge_Analytica \[Acedido: Jul. 2026\]

\[3\] Huntress, "Facebook Cambridge Scandal Data Breach: What Happened, Impact, and Lessons," *Huntress*, Nov. 2025. \[Online\]. Disponível em: https://www.huntress.com/threat-library/data-breach/cambridge-facebook-scandal-data-breach \[Acedido: Jul. 2026\]

\[4\] CNBC, "Facebook-Cambridge Analytica: A timeline of the data hijacking scandal," *CNBC*, Abr. 2018. \[Online\]. Disponível em: https://www.cnbc.com/2018/04/10/facebook-cambridge-analytica-a-timeline-of-the-data-hijacking-scandal.html \[Acedido: Jul. 2026\]

\[5\] FTC, "FTC Imposes $5 Billion Penalty and Sweeping New Privacy Restrictions on Facebook," *Federal Trade Commission*, Jul. 2019. \[Online\]. Disponível em: https://www.ftc.gov/news-events/news/press-releases/2019/07/ftc-imposes-5-billion-penalty-sweeping-new-privacy-restrictions-facebook \[Acedido: Jul. 2026\]

\[6\] FTC, "FTC Sues Cambridge Analytica, Settles with Former CEO and App Developer," *Federal Trade Commission*, Jul. 2019. \[Online\]. Disponível em: https://www.ftc.gov/news-events/news/press-releases/2019/07/ftc-sues-cambridge-analytica-settles-former-ceo-app-developer \[Acedido: Jul. 2026\]

\[7\] Amnesty International, "'The Great Hack': Cambridge Analytica is just the tip of the iceberg," *Amnesty International*, Jul. 2019. \[Online\]. Disponível em: https://www.amnesty.org/en/latest/news/2019/07/the-great-hack-facebook-cambridge-analytica/ \[Acedido: Jul. 2026\]

\[8\] Jackson School of International Studies, "Facebook and Data Privacy in the Age of Cambridge Analytica," *University of Washington*, 2019. \[Online\]. Disponível em: https://jsis.washington.edu/news/facebook-data-privacy-age-cambridge-analytica/ \[Acedido: Jul. 2026\]

\[9\] The Daily Scrum News, "Facebook's VPN Scandal: The Acquisition Of Onavo Spied On Millions," *The Daily Scrum News*, Ago. 2025. \[Online\]. Disponível em: https://www.thedailyscrumnews.com/facebooks-vpn-scandal-the-acquisition-of-onavo-spied-on-millions/ \[Acedido: Jul. 2026\]

\[10\] TechCrunch, "Apple removed Facebook's Onavo from the App Store for gathering app data," *TechCrunch*, Ago. 2018. \[Online\]. Disponível em: https://techcrunch.com/2018/08/22/apple-facebook-onavo/ \[Acedido: Jul. 2026\]

\[11\] CNBC, "Apple removes Facebook's Onavo security app from the App Store," *CNBC*, Ago. 2018. \[Online\]. Disponível em: https://www.cnbc.com/2018/08/22/apple-removes-facebook-onavo-app-from-app-store.html \[Acedido: Jul. 2026\]

\[12\] MobileSyrup, "Facebook removes Onavo Protect VPN from iOS App Store," *MobileSyrup*, Ago. 2018. \[Online\]. Disponível em: https://mobilesyrup.com/2018/08/23/facebook-removes-onavo-protect-vpn-from-ios-app-store/ \[Acedido: Jul. 2026\]

\[13\] Greater Manchester Combined Authority, "The Cambridge Analytica Scandal and What It Teaches Us," *Greater Manchester*. \[Online\]. Disponível em: https://greatermanchester.ac.uk/blogs/the-cambridge-analytica-scandal-and-what-it-teaches-us/ \[Acedido: Jul. 2026\]

\[14\] Forbes, "FTC Slaps Facebook With $5 Billion Fine, Forces New Privacy Controls," *Forbes*, Jul. 2019. \[Online\]. Disponível em: https://www.forbes.com/sites/mnunez/2019/07/24/ftcs-unprecedented-slap-fines-facebook-5-billion-forces-new-privacy-controls/ \[Acedido: Jul. 2026\]

\[15\] TutorChase, "How does the HTTP protocol transmit data packets?" *TutorChase*. \[Online\]. Disponível em: https://www.tutorchase.com/answers/ib/computer-science/how-does-the-http-protocol-transmit-data-packets \[Acedido: Jul. 2026\]

\[16\] N. Pubudu, "Understanding HTTP Protocol & OSI Model," *Medium - Geek Culture*, Mai. 2021. \[Online\]. Disponível em: https://medium.com/geekculture/understanding-http-protocol-osi-model-ba57cd5bda14 \[Acedido: Jul. 2026\]

\[17\] TechTarget, "What is HTTP and how does it work? Hypertext Transfer Protocol," *TechTarget*. \[Online\]. Disponível em: https://www.techtarget.com/whatis/definition/HTTP-Hypertext-Transfer-Protocol \[Acedido: Jul. 2026\]

\[18\] C. Cristian, "How Data Travels Through the Internet; explained in a simple day-to-day language," *Medium*, Jul. 2023. \[Online\]. Disponível em: https://medium.com/@cristian_74335/how-data-travels-through-the-internet-explained-in-a-simple-day-to-day-language-3923a0afe56a \[Acedido: Jul. 2026\]

\[19\] http.dev, "HTTP Explained," *http.dev*. \[Online\]. Disponível em: https://http.dev/explained \[Acedido: Jul. 2026\]

\[20\] Web Asha Technologies, "Wireshark Explained | Mastering Packet Analysis for Ethical Hacking," *Web Asha Technologies*, Dez. 2024. \[Online\]. Disponível em: https://www.webasha.com/blog/wireshark-explained-mastering-packet-analysis-for-ethical-hacking \[Acedido: Jul. 2026\]

\[21\] GeeksforGeeks, "Wireshark - Packet Capturing and Analyzing," *GeeksforGeeks*. \[Online\]. Disponível em: https://www.geeksforgeeks.org/computer-networks/wireshark-packet-capturing-and-analyzing/ \[Acedido: Jul. 2026\]

\[22\] K. Brian, "Packet Capture Tools: Using Wireshark to Determine Malicious Activity," *Kandi Brian - Cybersecurity Instructor*, Fev. 2026. \[Online\]. Disponível em: https://kandibrian.com/articles/wireshark-packet-capture-tools.html \[Acedido: Jul. 2026\]

\[23\] StationX, "How to Use Wireshark to Capture Network Traffic (2026)," *StationX*, Jan. 2026. \[Online\]. Disponível em: https://www.stationx.net/how-to-use-wireshark-to-capture-network-traffic/ \[Acedido: Jul. 2026\]

\[24\] Bridge Connect, "Understanding Symmetric Key Cryptography: A Beginner's Guide," *Bridge Connect*, Ago. 2025. \[Online\]. Disponível em: https://www.bridge-connect.com/post/understanding-symmetric-key-cryptography-a-beginner-s-guide \[Acedido: Jul. 2026\]

\[25\] GeeksforGeeks, "Symmetric Key Cryptography," *GeeksforGeeks*. \[Online\]. Disponível em: https://www.geeksforgeeks.org/computer-networks/symmetric-key-cryptography/ \[Acedido: Jul. 2026\]

\[26\] Device Authority, "Symmetric Encryption vs Asymmetric Encryption: How it Works and Why it's Used," *Device Authority*, Nov. 2024. \[Online\]. Disponível em: https://deviceauthority.com/symmetric-encryption-vs-asymmetric-encryption/ \[Acedido: Jul. 2026\]

\[27\] Zeeve, "An Exploration of Symmetric Key Cryptography: History, Working, and Applications," *Zeeve*, Out. 2025. \[Online\]. Disponível em: https://www.zeeve.io/blog/an-exploration-of-symmetric-key-cryptography-history-working-and-applications/ \[Acedido: Jul. 2026\]

\[28\] Cornell University - CS5430, "Symmetric-Key Cryptography," *Cornell University*, 2025. \[Online\]. Disponível em: https://www.cs.cornell.edu/courses/cs5430/2025sp/TL03.symmetric.html \[Acedido: Jul. 2026\]

\[29\] MDN Web Docs, "Overview of HTTP," *Mozilla Developer Network*. \[Online\]. Disponível em: https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/Overview \[Acedido: Jul. 2026\]

\[30\] Paessler, "What Is HTTP?," *PRTG / Paessler*. \[Online\]. Disponível em: https://www.paessler.com/it-explained/http \[Acedido: Jul. 2026\]

\[31\] SecurityScorecard, "How Does Wireshark Improve Network Security Through Packet Analysis?," *SecurityScorecard*, Abr. 2026. \[Online\]. Disponível em: https://securityscorecard.com/blog/how-does-wireshark-improve-network-security-through-packet-analysis/ \[Acedido: Jul. 2026\]

\[32\] EC-Council, "What is Wireshark? Network Packet Capturing and Analysis with Wireshark," *EC-Council*, Mar. 2026. \[Online\]. Disponível em: https://www.eccouncil.org/cybersecurity-exchange/penetration-testing/wireshark-packet-capturing-analysis/ \[Acedido: Jul. 2026\]

\[33\] Wikipedia, "Open-source intelligence," *Wikipedia, The Free Encyclopedia*. \[Online\]. Disponível em: https://en.wikipedia.org/wiki/Open-source_intelligence \[Acedido: Jul. 2026\]

\[34\] Rapid7, "What is Open Source Intelligence (OSINT)?," *Rapid7*. \[Online\]. Disponível em: https://www.rapid7.com/fundamentals/what-is-open-source-intelligence-osint/ \[Acedido: Jul. 2026\]

\[35\] Moody's, "How to use Open-Source Intelligence (OSINT) for investigations," *Moody's*, Dez. 2025. \[Online\]. Disponível em: https://www.moodys.com/web/en/us/insights/compliance-tprm/open-source-intelligence-osint-types-tools-and-methods.html \[Acedido: Jul. 2026\]

\[36\] SANS Institute, "What is OSINT (Open-Source Intelligence)?," *SANS Institute*, Set. 2025. \[Online\]. Disponível em: https://www.sans.org/blog/what-is-open-source-intelligence \[Acedido: Jul. 2026\]

\[37\] Imperva, "Open-Source Intelligence (OSINT): Techniques & Tools," *Imperva*. \[Online\]. Disponível em: https://www.imperva.com/learn/application-security/open-source-intelligence-osint/ \[Acedido: Jul. 2026\]

\[38\] Bitdefender, "What is Social Engineering," *Bitdefender InfoZone*, Jan. 2025. \[Online\]. Disponível em: https://www.bitdefender.com/en-us/business/infozone/what-is-social-engineering \[Acedido: Jul. 2026\]

\[39\] Social-Engineer.org, "Physical Methods of Information Gathering," *Security Through Education*, Fev. 2022. \[Online\]. Disponível em: https://www.social-engineer.org/framework/information-gathering/physical-methods-of-information-gathering/ \[Acedido: Jul. 2026\]

\[40\] ACM / Communications of the ACM, "Duped No More: Navigating the Maze of Social Engineering Schemes," *Communications of the ACM*, Set. 2023. \[Online\]. Disponível em: https://cacm.acm.org/blogcacm/duped-no-more-navigating-the-maze-of-social-engineering-schemes/ \[Acedido: Jul. 2026\]

\[41\] Tripwire, "Beyond the firewall: How social engineers use psychology to compromise organizational cybersecurity," *Tripwire - State of Security*, Mai. 2023. \[Online\]. Disponível em: https://www.tripwire.com/state-of-security/beyond-firewall-how-social-engineers-use-psychology-compromise-organizational \[Acedido: Jul. 2026\]

\[42\] Neotas, "OSINT Tools And Techniques," *Neotas*, Jun. 2026. \[Online\]. Disponível em: https://www.neotas.com/osint-tools-and-techniques/ \[Acedido: Jul. 2026\]

\[43\] Blackdot Solutions, "5 Open Source Intelligence Gathering Techniques," *Blackdot Solutions*, Nov. 2025. \[Online\]. Disponível em: https://blackdotsolutions.com/blog/open-source-intelligence-techniques \[Acedido: Jul. 2026\]

\[44\] ShadowDragon, "OSINT Techniques: Expert Tactics for Investigators (2026)," *ShadowDragon*, Abr. 2026. \[Online\]. Disponível em: https://shadowdragon.io/resources/osint-techniques/ \[Acedido: Jul. 2026\]

\[45\] University of Florida - Information Security, "Dumpster Diving," *UF Information Technology*. \[Online\]. Disponível em: https://it.ufl.edu/security/learn-security/social-engineering/dumpster-diving/ \[Acedido: Jul. 2026\]

\[46\] GRC Viewpoint, "The watering hole, piggybacking, dumpster diving, and other social engineering attacks threatening employees," *GRC Viewpoint*, Jul. 2025. \[Online\]. Disponível em: https://www.grcviewpoint.com/the-watering-hole-piggybacking-dumpster-diving-and-other-social-engineering-attacks-threatening-employees/ \[Acedido: Jul. 2026\]

\[47\] MDPI - Future Internet, "Social Engineering Attacks: A Survey," *Future Internet*, vol. 11, n.o 4, 2019. \[Online\]. Disponível em: https://www.mdpi.com/1999-5903/11/4/89 \[Acedido: Jul. 2026\]


