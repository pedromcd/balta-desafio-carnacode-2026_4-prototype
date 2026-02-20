namespace DesignPatternChallenge
{
    // ============================
    // 1) PROTOTYPE (Interface)
    // ============================
    public interface IPrototype<T>
    {
        T Clone();
    }

    // ============================
    // 2) MODELOS DO DOCUMENTO
    // ============================
    public class DocumentTemplate : IPrototype<DocumentTemplate>
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public List<Section> Sections { get; set; } = new();
        public DocumentStyle Style { get; set; }
        public List<string> RequiredFields { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public ApprovalWorkflow Workflow { get; set; }
        public List<string> Tags { get; set; } = new();

        // Deep copy (copia tudo, sem compartilhar referências)
        public DocumentTemplate Clone()
        {
            return new DocumentTemplate
            {
                Title = this.Title,
                Category = this.Category,

                Style = this.Style?.Clone(),
                Workflow = this.Workflow?.Clone(),

                Sections = this.Sections.Select(s => s.Clone()).ToList(),
                RequiredFields = new List<string>(this.RequiredFields),
                Metadata = new Dictionary<string, string>(this.Metadata),
                Tags = new List<string>(this.Tags)
            };
        }
    }

    public class Section : IPrototype<Section>
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public bool IsEditable { get; set; }
        public List<string> Placeholders { get; set; } = new();

        public Section Clone()
        {
            return new Section
            {
                Name = this.Name,
                Content = this.Content,
                IsEditable = this.IsEditable,
                Placeholders = new List<string>(this.Placeholders)
            };
        }
    }

    public class DocumentStyle : IPrototype<DocumentStyle>
    {
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public string HeaderColor { get; set; }
        public string LogoUrl { get; set; }
        public Margins PageMargins { get; set; }

        public DocumentStyle Clone()
        {
            return new DocumentStyle
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                HeaderColor = this.HeaderColor,
                LogoUrl = this.LogoUrl,
                PageMargins = this.PageMargins?.Clone()
            };
        }
    }

    public class Margins : IPrototype<Margins>
    {
        public int Top { get; set; }
        public int Bottom { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }

        public Margins Clone()
        {
            return new Margins
            {
                Top = this.Top,
                Bottom = this.Bottom,
                Left = this.Left,
                Right = this.Right
            };
        }
    }

    public class ApprovalWorkflow : IPrototype<ApprovalWorkflow>
    {
        public List<string> Approvers { get; set; } = new();
        public int RequiredApprovals { get; set; }
        public int TimeoutDays { get; set; }

        public ApprovalWorkflow Clone()
        {
            return new ApprovalWorkflow
            {
                RequiredApprovals = this.RequiredApprovals,
                TimeoutDays = this.TimeoutDays,
                Approvers = new List<string>(this.Approvers)
            };
        }
    }

    // ============================
    // 3) SERVIÇO USANDO PROTÓTIPOS
    // ============================
    public class DocumentService
    {
        private readonly DocumentTemplate _serviceContractPrototype;
        private readonly DocumentTemplate _consultingContractPrototype;

        public DocumentService()
        {
            // Custo de criação acontece 1 vez (protótipos)
            _serviceContractPrototype = CreateBaseServiceContractPrototype();
            _consultingContractPrototype = CreateConsultingFromServicePrototype(_serviceContractPrototype);
        }

        // Cria protótipo base (simula inicialização custosa)
        private DocumentTemplate CreateBaseServiceContractPrototype()
        {
            Console.WriteLine("Inicializando protótipo: Contrato de Serviço (uma única vez)...");
            Thread.Sleep(100); // simula custo

            var template = new DocumentTemplate
            {
                Title = "Contrato de Prestação de Serviços",
                Category = "Contratos",
                Style = new DocumentStyle
                {
                    FontFamily = "Arial",
                    FontSize = 12,
                    HeaderColor = "#003366",
                    LogoUrl = "https://company.com/logo.png",
                    PageMargins = new Margins { Top = 2, Bottom = 2, Left = 3, Right = 3 }
                },
                Workflow = new ApprovalWorkflow
                {
                    RequiredApprovals = 2,
                    TimeoutDays = 5,
                    Approvers = new List<string> { "gerente@empresa.com", "juridico@empresa.com" }
                }
            };

            template.Sections.Add(new Section
            {
                Name = "Cláusula 1 - Objeto",
                Content = "O presente contrato tem por objeto...",
                IsEditable = true
            });
            template.Sections.Add(new Section
            {
                Name = "Cláusula 2 - Prazo",
                Content = "O prazo de vigência será de...",
                IsEditable = true
            });
            template.Sections.Add(new Section
            {
                Name = "Cláusula 3 - Valor",
                Content = "O valor total do contrato é de...",
                IsEditable = true
            });

            template.RequiredFields.AddRange(new[] { "NomeCliente", "CPF", "Endereco" });
            template.Tags.AddRange(new[] { "contrato", "servicos" });

            template.Metadata["Versao"] = "1.0";
            template.Metadata["Departamento"] = "Comercial";
            template.Metadata["UltimaRevisao"] = DateTime.Now.ToString("s");

            return template;
        }

        // Deriva um template similar a partir do protótipo (sem duplicar tudo)
        private DocumentTemplate CreateConsultingFromServicePrototype(DocumentTemplate servicePrototype)
        {
            Console.WriteLine("Derivando protótipo: Contrato de Consultoria (a partir do protótipo base)...");
            var consulting = servicePrototype.Clone();

            consulting.Title = "Contrato de Consultoria";
            consulting.Tags.Remove("servicos");
            consulting.Tags.Add("consultoria");

            // Ajusta só o que muda
            var clause1 = consulting.Sections.FirstOrDefault(s => s.Name.Contains("Cláusula 1"));
            if (clause1 != null)
                clause1.Content = "O presente contrato de consultoria tem por objeto...";

            // Exemplo: consultoria não precisa da cláusula 3
            consulting.Sections.RemoveAll(s => s.Name.Contains("Cláusula 3"));

            return consulting;
        }

        // API pública: criar docs clonando protótipos
        public DocumentTemplate CreateServiceContractForClient(int clientId)
        {
            var doc = _serviceContractPrototype.Clone();
            doc.Title = $"Contrato #{clientId} - Cliente {clientId}";
            doc.Metadata["ClienteId"] = clientId.ToString();
            doc.Metadata["GeradoEm"] = DateTime.Now.ToString("s");
            return doc;
        }

        public DocumentTemplate CreateConsultingContractForClient(int clientId)
        {
            var doc = _consultingContractPrototype.Clone();
            doc.Title = $"Contrato Consultoria #{clientId} - Cliente {clientId}";
            doc.Metadata["ClienteId"] = clientId.ToString();
            doc.Metadata["GeradoEm"] = DateTime.Now.ToString("s");
            return doc;
        }

        public void DisplayTemplate(DocumentTemplate template)
        {
            Console.WriteLine($"\n=== {template.Title} ===");
            Console.WriteLine($"Categoria: {template.Category}");
            Console.WriteLine($"Seções: {template.Sections.Count}");
            Console.WriteLine($"Campos obrigatórios: {string.Join(", ", template.RequiredFields)}");
            Console.WriteLine($"Aprovadores: {string.Join(", ", template.Workflow.Approvers)}");
            Console.WriteLine($"Tags: {string.Join(", ", template.Tags)}");
        }
    }

    // ============================
    // 4) DEMO
    // ============================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Templates (Prototype) ===\n");

            var service = new DocumentService();

            Console.WriteLine("\nCriando 5 contratos de serviço via clonagem...");
            var startTime = DateTime.Now;

            for (int i = 1; i <= 5; i++)
            {
                var contract = service.CreateServiceContractForClient(i);
                // personalização pós-clonagem
            }

            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            Console.WriteLine($"Tempo total: {elapsed}ms\n");

            var consulting = service.CreateConsultingContractForClient(1);
            service.DisplayTemplate(consulting);

            Console.ReadLine();
        }
    }
}