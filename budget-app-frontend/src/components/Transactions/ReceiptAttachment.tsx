import React, { useState } from 'react';
import axios from 'axios';

interface ReceiptAttachmentProps {
  transactionId: number;
  receiptUrl?: string;
}

const ReceiptAttachment: React.FC<ReceiptAttachmentProps> = ({ transactionId, receiptUrl }) => {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [receipt, setReceipt] = useState<string | undefined>(receiptUrl);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    setUploading(true);
    setError(null);
    const formData = new FormData();
    formData.append('transactionId', transactionId.toString());
    formData.append('file', file);
    try {
      const res = await axios.post('/api/receipt-attachments/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setReceipt(`/api/receipt-attachments/${res.data.id}`);
    } catch (err: any) {
      setError('Kunde inte ladda upp kvitto.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <h4>Kvitto</h4>
      {receipt ? (
        <a href={receipt} target="_blank" rel="noopener noreferrer">Visa kvitto</a>
      ) : (
        <div>
          <input type="file" accept="image/*,application/pdf" onChange={handleFileChange} />
          <button onClick={handleUpload} disabled={uploading || !file}>
            {uploading ? 'Laddar upp...' : 'Ladda upp kvitto'}
          </button>
          {error && <div style={{ color: 'red' }}>{error}</div>}
        </div>
      )}
    </div>
  );
};

export default ReceiptAttachment;
