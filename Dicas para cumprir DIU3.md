## **1\. Instruções e Dicas Gerais para Cumprir o Desafio**

**1️⃣ Entenda a proposta antes de codar**

* Revise seu jogo do DIU2: o que já funciona bem? o que falta para gerar *experiência*?  
* Defina **claramente** qual emoção ou experiência quer provocar (ex: tensão, diversão, cuidado, curiosidade).  
* Escreva em uma frase o *core experience loop* do seu jogo, tipo:

   “Quero que o jogador sinta tensão enquanto tenta sobreviver o máximo possível, coletando recursos antes que o tempo acabe.”

---

**2️⃣ Planeje melhorias com base nos critérios de pontuação**

* Faça uma checklist com os itens obrigatórios:

  * Menus (inicial, pause, game over)  
  * Sistema de recompensas (coleta, pontuação, bônus, punição)  
  * Câmera especial (Cinemachine) **OU** inimigo com IA básica  
  * Ação especial com custo de recurso  
  * Experiência divertida (criatividade\!)

---

**3️⃣ Priorize a jogabilidade, não a estética**

* Gráficos simples, mas **gameplay sólido**.  
* Use assets prontos apenas para completar o conceito — não perca tempo redesenhando tudo.

---

**4️⃣ Pense no “loop de diversão”**  
 Um bom jogo casual tem:

* Um objetivo **claro** (ex: sobreviver, coletar, evitar algo);  
* Um **risco constante** (vida, tempo, energia);  
* Um **recompensa cíclica** (pontos, bônus, progressão);  
* Feedback visual/sonoro imediato.

---

**5️⃣ Teste continuamente**

* Rode a cena frequentemente (Ctrl+P).  
* Grave bugs e correções num caderninho ou README.  
* Peça a alguém para jogar e anote onde ele fica confuso.

---

**6️⃣ Prepare o material final com calma**

* Grave o vídeo apresentando: menus, jogabilidade, recompensas e a experiência pensada.  
* Publique o WebGL ([https://play.unity.com/](https://play.unity.com/)) e o repositório no GitHub.

---

## **🧭 2\. Roteiro de Aprendizado e Execução – 7 dias**

### **🗓 Dia 1 – Revisão e Planejamento**

* Revisite seu projeto do DIU2.  
* Identifique o que já existe (movimento, colisão, cenário).  
* Defina **a experiência principal** (ex: medo, diversão, desafio).  
* Faça uma lista de tarefas técnicas e criativas (To-Do).

### **🗓 Dia 2 – Sistema de Recompensas**

* Escolha o tipo:  
  * Coleta (moedas, energia, pontos);  
  * Derrota de inimigos;  
  * Aumento de tempo/vida;  
* Implemente contadores (UI Text / TMP).  
* Dica: use `OnTriggerEnter2D` para detectar coleta e aumentar pontos.

### **🗓 Dia 3 – Menus e Estados do Jogo**

* Crie cenas: `MainMenu`, `GameScene`, `GameOver`.  
* Use `SceneManager.LoadScene()` para trocar de cena.  
* Adicione um sistema de *Pause* (esc \+ Time.timeScale \= 0).  
* Teste transições e feedbacks visuais simples.

### **🗓 Dia 4 – Recurso e Ação Especial**

* Escolha um recurso limitado (ex: energia, mana, tempo).  
* Crie uma ação especial (ex: explosão, escudo, golpe forte).  
* Faça a ação consumir recurso (ex: `energia -= 10;`).  
* Mostre o custo visualmente (barra de energia ou som).

### **🗓 Dia 5 – Câmera ou IA do Inimigo**

* **Opção 1 (Cinemachine):**

  * Instale o pacote Cinemachine.  
  * Adicione um *Virtual Camera* que segue o jogador.

* **Opção 2 (IA):**

  * Use o pacote *NavMesh* ou implemente *pathfinding* simples com `Vector2.MoveTowards()`.  
  * Inimigo persegue jogador ou patrulha área.

### **🗓 Dia 6 – Polimento e Testes**

* Adicione sons e feedbacks visuais.  
* Ajuste colisões, velocidades e espaçamentos.  
* Teste repetidamente o ciclo “morrer – recomeçar – progredir”.  
* Escreva um pequeno README com instruções.

### **🗓 Dia 7 – Entrega e Apresentação**

* Grave vídeo curto (até 10 min):  
  * Mostre menus, gameplay, e explique decisões de design.  
  * Fale sobre o que aprendeu e desafios técnicos.  
* Suba no GitHub e publique o WebGL.  
* Revise se todos os links funcionam.

---

## **💡 Sugestões extras**

* Use o **Tilemap** para criar uma fase com ritmo visual (plataformas, obstáculos, limites).  
* Explore um microgame do Unity Learn ([https://learn.unity.com/pathway/unity-essentials](https://learn.unity.com/pathway/unity-essentials)) para inspiração rápida.  
* Se tiver pouco tempo, **priorize menus \+ sistema de recompensas \+ experiência clara** — esses três pontos dão mais de 60% da nota.

