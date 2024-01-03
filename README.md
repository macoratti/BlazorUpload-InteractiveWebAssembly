A renderização do lado do cliente (CSR) renderiza o componente interativamente no cliente usando Blazor WebAssembly. 

O runtime do .NET e o pacote de aplicativos são baixados e armazenados em cache quando o componente WebAssembly é renderizado inicialmente. 

Os componentes que usam CSR devem ser criados a partir de um projeto cliente separado que configura o host Blazor WebAssembly.
Ao criar um "Blazor Web App" com uma interatividade que inclui Wasm você já obtém um projeto Cliente e um Servidor. 

O projeto Shared (DTO) está faltando, mas você pode adicioná-lo facilmente, é apenas uma biblioteca de classes.Adicione
AddControllers() e MapControllers() ao Program.cs do servidor e então você pode começar a adicionar os controladores necessários.

Adicione AddControllers() e MapControllers() ao Program.cs do servidor e então você pode começar a adicionar os controladores necessários.

builder.Services.AddControllers();

var app = builder.Build();
   ...
app.MapControllers();
app.Run();

Todos os componentes interativos que forem rodar no cliente devem ser criados no projeto WebAssembly
