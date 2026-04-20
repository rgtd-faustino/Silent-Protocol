# Silent Protocol - Relatório de Projeto

**Instituto Superior de Engenharia de Lisboa (ISEL)**
**Departamento de Engenharia Eletrónica e de Telecomunicações e Computadores (DEETC)**
**LEIM - Licenciatura em Engenharia Informática e Multimédia**

---

**Título do Trabalho:** Silent Protocol: Desenvolvimento de um Jogo de Stealth e Espionagem Corporativa
**Nome do Aluno:** [Nome do Aluno] ([Número])
**Orientadores:** [Professor Doutor Nome do Orientador]

**Mês, Ano**

---

### Resumo [i]
Este trabalho descreve o desenvolvimento do projeto **Silent Protocol**, um jogo de sigilo (*stealth*) focado em ambiente de escritório e espionagem corporativa. O objetivo principal foi criar um sistema de jogo onde o jogador deve realizar tarefas administrativas triviais enquanto executa ações de espionagem, gerindo um nível de suspeita que aumenta com a proximidade de NPCs e falhas em prazos. O projeto destaca-se pela implementação de uma Inteligência Artificial reativa baseada em máquinas de estado e um sistema de eventos centralizado que garante o desacoplamento dos sistemas de jogo. Os resultados demonstram um ciclo de jogo funcional com mecânicas de risco-recompensa imersivas.

**Palavras-chave:** Unity, Stealth, IA, Máquinas de Estado, Event-Driven Architecture.

---

### Abstract [iii]
This report describes the development of **Silent Protocol**, a stealth-focused game centered on office environments and corporate espionage. The primary goal was to create a gameplay system where players must perform mundane administrative tasks while carrying out espionage actions, managing a suspicion level that increases with NPC proximity and missed deadlines. The project features a reactive AI system based on state machines and a centralized event-driven architecture that ensures system decoupling. Results demonstrate a functional gameplay loop with immersive risk-reward mechanics.

**Keywords:** Unity, Stealth, AI, State Machines, Event-Driven Architecture.

---

### Agradecimentos [v]
[Escrever aqui eventuais agradecimentos a professores, orientadores, colegas e familiares que contribuíram para a realização deste projeto.]

---

### Índice [ix]
- [Resumo](#resumo-i) [i]
- [Abstract](#abstract-iii) [iii]
- [Agradecimentos](#agradecimentos-v) [v]
- [Índice](#índice-ix) [ix]
- [Lista de Tabelas](#lista-de-tabelas-xi) [xi]
- [Lista de Figuras](#lista-de-figuras-xiii) [xiii]
- [1 Introdução](#1-introdução-1) [1]
- [2 Trabalho Relacionado](#2-trabalho-relacionado-3) [3]
- [3 Modelo Proposto](#3-modelo-proposto-5) [5]
    - [3.1 Requisitos](#31-requisitos) [5]
    - [3.2 Fundamentos](#32-fundamentos) [5]
    - [3.3 Abordagem](#33-abordagem) [6]
- [4 Implementação do Modelo](#4-implementação-do-modelo-7) [7]
- [5 Validação e Testes](#5-validação-e-testes-9) [9]
- [6 Conclusões e Trabalho Futuro](#6-conclusões-e-trabalho-futuro-11) [11]
- [Apêndice A - Um Detalhe Adicional](#apêndice-a-um-detalhe-adicional-13) [13]
- [Apêndice B - Outro Detalhe Adicional](#apêndice-b-outro-detalhe-adicional-15) [15]
- [Bibliografia](#bibliografia-17) [17]

---

### Lista de Tabelas [xi]
- 5.1 Uma tabela . . . . . . . . . . . . . . . . . . . . . . . . . . . . 9

---

### Lista de Figuras [xiii]
- 5.1 Uma figura . . . . . . . . . . . . . . . . . . . . . . . . . . . . 9

---

### 1 Introdução [1]
O género de jogos de *stealth* evoluiu significativamente, passando de mecânicas simples de visibilidade para sistemas complexos de simulação social e ambiental. O projeto **Silent Protocol** surge da necessidade de explorar a tensão entre a rotina de trabalho normal e a atividade clandestina.

Neste trabalho, é apresentada a motivação, as ideias essenciais e os contributos técnicos para a criação de uma experiência de jogo imersiva. O leitor encontrará detalhes sobre a arquitetura de eventos, o sistema de IA reativa e a integração de mecânicas de gestão de suspeita.

---

### 2 Trabalho Relacionado [3]
O projeto insere-se no contexto de jogos de simulação social e sigilo. Foram analisados trabalhos que definem os pressupostos teóricos e tecnológicos:

- **Sistemas de Sigilo Social**: Jogos como *Hitman* utilizam a "camuflagem social" como mecânica central.
- **IA e Navegação**: O uso de *NavMesh* e máquinas de estado finitas (FSM) é padrão na indústria para comportamentos reativos.
- **Arquitetura Event-Driven**: Padrão utilizado para desacoplar sistemas de UI e lógica de jogo.

---

### 3 Modelo Proposto [5]
O modelo proposto para o **Silent Protocol** foca-se no equilíbrio entre tarefas de escritório e ações de espionagem.

#### 3.1 Requisitos [5]
- **Funcionais**: Movimento com estados de postura (andar/agachar), sistema de suspeita reativo à proximidade de NPCs, e gestão de tarefas com prazos (Morning/Afternoon).
- **Não Funcionais**: Performance estável a 60 FPS e desacoplamento total entre UI e lógica via barramento de eventos.

#### 3.2 Fundamentos [5]
O sustento formal baseia-se num sistema de eventos centralizado (`GameEvent.cs`) que coordena o ciclo de dia/noite e os estados de alerta globais. A lógica de IA utiliza algoritmos de FOV baseados em produto escalar para deteção.

#### 3.3 Abordagem [6]
Explicação das formulações e algoritmos desenvolvidos. A abordagem utiliza corrotinas otimizadas para verificações de FOV a cada 0.1s, garantindo reatividade sem sobrecarga de CPU.

---

### 4 Implementação do Modelo [7]
A implementação foi realizada em Unity 2022.3 LTS usando C#. Os componentes principais incluem:
- **PlayerController**: Gestão de física e ruído.
- **NPCScript**: Máquina de estados para patrulha, investigação e perseguição.
- **SuspicionManager**: Gestão do medidor de alerta global.
- **TaskManager**: Lógica de geração e validação de objetivos administrativos.

---

### 5 Validação e Testes [9]
A validação incluiu testes funcionais do sistema de deteção e usabilidade da interface.

**Tabela 5.1: Dados de Teste de Deteção**
| Distância (m) | Incremento Suspeita | Reação NPC |
| :--- | :--- | :--- |
| < 5.0 | 2.0x | Perseguição Imediata |
| 5.0 - 10.0 | 1.5x | Investigação |
| > 10.0 | 1.0x | Alerta Visual |

**Figura 5.1: Ciclo de Estados da IA**
[Placeholder para diagrama de estados da IA]

---

### 6 Conclusões e Trabalho Futuro [11]
O projeto demonstrou a eficácia de uma arquitetura modular para jogos de sigilo. A principal conclusão é que a gestão de suspeita baseada em tarefas cria uma camada de tensão extra além da deteção visual simples. Como trabalho futuro, prevê-se a implementação de sistemas de distração sonora e iluminação dinâmica.

---

### Apêndice A - Um Detalhe Adicional [13]
**Ciclo de Versões e Integração Contínua**
Explicação do processo de gestão de ramos (*branches*) no Git para garantir que funcionalidades como o sistema de café ou bateria não corrompam o ramo principal.

---

### Apêndice B - Outro Detalhe Adicional [15]
**Configuração do NavMesh e Obstáculos Dinâmicos**
Detalhes sobre como o cenário de escritório foi configurado com áreas de custo elevado para NPCs e como os obstáculos dinâmicos (cadeiras, portas) afetam o cálculo de caminhos em tempo real.

---

### Bibliografia [17]
[Bellifemine et al., 2007] Bellifemine, F. L., Caire, G., e Greenwood, D. (2007). Developing Multi-Agent Systems with JADE. Wiley Series in Agent Technology. Wiley.

[Boutilier et al., 1995] Boutilier, C., Dearden, R., e Goldszmidt, M. (1995). Exploiting structure in policy construction. In Proceedings of the IJCAI-95, p. 1104–1111.

[Elzinga e Mills, 2011] Elzinga, K. e Mills, D. (2011). The lerner index of monopoly power: Origins and uses. American Economic Review: Papers & Proceedings, 101(3).

[ISCTE, site] ISCTE (site). Ciberdúvidas da língua portuguesa. https://ciberduvidas.iscte-iul.pt/.

[Python3.2.3, 2012] Python3.2.3 (2012). Python programming language. http://docs.python.org/py3k/.

[Wooldridge, 2000] Wooldridge, M. (2000). Reasoning About Rational Agents, cap.: Implementing Rational Agents. The MIT Press.
