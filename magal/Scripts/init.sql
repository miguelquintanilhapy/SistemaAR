-- ==============================================================================
-- SISTEMA: ICE CAR - ORÇAMENTOS
-- ESTRUTURA DO BANCO DE DADOS
-- ==============================================================================

CREATE TABLE IF NOT EXISTS orcamento (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_nome VARCHAR(255) NOT NULL,
    cliente_telefone VARCHAR(30),
    veiculo_marca VARCHAR(100),
    veiculo_modelo VARCHAR(100),
    veiculo_placa VARCHAR(20),
    veiculo_cor VARCHAR(50),
    veiculo_ano INT,
    garantia_dias INT DEFAULT 90,
    observacoes TEXT,
    valor_total DECIMAL(18, 2) DEFAULT 0,
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS item_orcamento (
    id INT AUTO_INCREMENT PRIMARY KEY,
    orcamento_id INT NOT NULL,
    descricao VARCHAR(255) NOT NULL,
    valor_unitario DECIMAL(18, 2) NOT NULL,
    CONSTRAINT fk_item_orcamento FOREIGN KEY (orcamento_id) REFERENCES orcamento(id) ON DELETE CASCADE
);
