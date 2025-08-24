import React, { useEffect, useState } from 'react';
import { Typography, Box, Table, TableHead, TableRow, TableCell, TableBody } from '@mui/material';
import axios from 'axios';
import ReceiptAttachment from './ReceiptAttachment';

interface Transaction {
  id: number;
  date: string;
  description: string;
  amount: number;
  categoryName?: string;
  receiptAttachmentId?: number;
}

const Transactions: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchTransactions = async () => {
      setLoading(true);
      try {
        const res = await axios.get('/api/transactions');
        setTransactions(res.data);
      } catch (err) {
        // Hantera fel
      } finally {
        setLoading(false);
      }
    };
    fetchTransactions();
  }, []);

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Transaktioner
      </Typography>
      {loading ? (
        <Typography>Laddar...</Typography>
      ) : (
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Datum</TableCell>
              <TableCell>Beskrivning</TableCell>
              <TableCell>Belopp</TableCell>
              <TableCell>Kategori</TableCell>
              <TableCell>Kvitto</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {transactions.map(tx => (
              <TableRow key={tx.id}>
                <TableCell>{tx.date}</TableCell>
                <TableCell>{tx.description}</TableCell>
                <TableCell>{tx.amount}</TableCell>
                <TableCell>{tx.categoryName}</TableCell>
                <TableCell>
                  <ReceiptAttachment transactionId={tx.id} receiptUrl={tx.receiptAttachmentId ? `/api/receipt-attachments/${tx.receiptAttachmentId}` : undefined} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </Box>
  );
};

export default Transactions;