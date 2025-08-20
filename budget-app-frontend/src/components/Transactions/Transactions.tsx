import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  List,
  ListItem,
  ListItemText,
  IconButton,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Input,
  Alert,
  Divider,
  ListItemIcon,
} from '@mui/material';
import {
  AttachFile as AttachFileIcon,
  Receipt as ReceiptIcon,
  Download as DownloadIcon,
  Delete as DeleteIcon,
  Image as ImageIcon,
  PictureAsPdf as PdfIcon,
} from '@mui/icons-material';
import { apiService } from '../../services/apiService';
import { Transaction, TransactionAttachment } from '../../types';

const Transactions: React.FC = () => {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [selectedTransaction, setSelectedTransaction] = useState<Transaction | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTransactions();
  }, []);

  const loadTransactions = async () => {
    try {
      const data = await apiService.getTransactions();
      setTransactions(data);
    } catch (error) {
      console.error('Error loading transactions:', error);
      setError('Kunde inte ladda transaktioner');
    } finally {
      setLoading(false);
    }
  };

  const handleTransactionClick = async (transaction: Transaction) => {
    try {
      // Get detailed transaction with attachments
      const detailedTransaction = await apiService.getTransaction(transaction.id);
      setSelectedTransaction(detailedTransaction);
      setDialogOpen(true);
    } catch (error) {
      console.error('Error loading transaction details:', error);
      setError('Kunde inte ladda transaktionsdetaljer');
    }
  };

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    if (!event.target.files || !selectedTransaction) return;

    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'];
    if (!allowedTypes.includes(file.type)) {
      setError('Endast bilder (JPEG, PNG, GIF) och PDF-filer är tillåtna');
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      setError('Filen är för stor (max 5MB)');
      return;
    }

    setUploading(true);
    setError(null);

    try {
      const attachment = await apiService.uploadAttachment(selectedTransaction.id, file);
      
      // Update the transaction with the new attachment
      const updatedTransaction = {
        ...selectedTransaction,
        attachments: [...(selectedTransaction.attachments || []), attachment]
      };
      setSelectedTransaction(updatedTransaction);

      // Update the transaction in the list
      setTransactions(prev => 
        prev.map(t => t.id === selectedTransaction.id ? updatedTransaction : t)
      );
    } catch (error: any) {
      setError(error.response?.data || 'Kunde inte ladda upp filen');
    } finally {
      setUploading(false);
      // Reset the input
      event.target.value = '';
    }
  };

  const handleDownloadAttachment = async (attachment: TransactionAttachment) => {
    try {
      const blob = await apiService.downloadAttachment(selectedTransaction!.id, attachment.id);
      
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = attachment.fileName;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (error) {
      setError('Kunde inte ladda ner filen');
    }
  };

  const handleDeleteAttachment = async (attachment: TransactionAttachment) => {
    if (!selectedTransaction) return;

    try {
      await apiService.deleteAttachment(selectedTransaction.id, attachment.id);
      
      // Update the transaction
      const updatedTransaction = {
        ...selectedTransaction,
        attachments: selectedTransaction.attachments?.filter(a => a.id !== attachment.id) || []
      };
      setSelectedTransaction(updatedTransaction);

      // Update the transaction in the list
      setTransactions(prev => 
        prev.map(t => t.id === selectedTransaction.id ? updatedTransaction : t)
      );
    } catch (error) {
      setError('Kunde inte ta bort bilagan');
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('sv-SE', {
      style: 'currency',
      currency: 'SEK'
    }).format(amount);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('sv-SE');
  };

  const getFileIcon = (contentType?: string) => {
    if (contentType?.startsWith('image/')) {
      return <ImageIcon />;
    } else if (contentType === 'application/pdf') {
      return <PdfIcon />;
    }
    return <AttachFileIcon />;
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  if (loading) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom>
          Transaktioner
        </Typography>
        <Typography>Laddar...</Typography>
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Transaktioner
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Card>
        <CardContent>
          {transactions.length === 0 ? (
            <Typography color="textSecondary">
              Inga transaktioner hittades.
            </Typography>
          ) : (
            <List>
              {transactions.map((transaction) => (
                <ListItem
                  key={transaction.id}
                  divider
                  sx={{ cursor: 'pointer' }}
                  onClick={() => handleTransactionClick(transaction)}
                >
                  <ListItemText
                    primary={transaction.description}
                    secondary={`${formatDate(transaction.date)} • ${transaction.account?.name || ''}`}
                  />
                  <Box textAlign="right" display="flex" alignItems="center" gap={1}>
                    <Box>
                      <Typography
                        variant="body2"
                        color={transaction.amount >= 0 ? 'success.main' : 'error.main'}
                      >
                        {formatCurrency(transaction.amount)}
                      </Typography>
                      {transaction.category && (
                        <Chip
                          label={transaction.category.name}
                          size="small"
                          sx={{ mt: 0.5 }}
                        />
                      )}
                    </Box>
                    {transaction.attachments && transaction.attachments.length > 0 && (
                      <ReceiptIcon color="action" />
                    )}
                  </Box>
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Transaction Details Dialog */}
      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>
          Transaktionsdetaljer
        </DialogTitle>
        <DialogContent>
          {selectedTransaction && (
            <Box>
              <Typography variant="h6" gutterBottom>
                {selectedTransaction.description}
              </Typography>
              <Typography variant="body2" color="textSecondary" gutterBottom>
                {formatDate(selectedTransaction.date)} • {selectedTransaction.account?.name}
              </Typography>
              <Typography
                variant="h5"
                color={selectedTransaction.amount >= 0 ? 'success.main' : 'error.main'}
                gutterBottom
              >
                {formatCurrency(selectedTransaction.amount)}
              </Typography>

              {selectedTransaction.category && (
                <Chip
                  label={selectedTransaction.category.name}
                  size="small"
                  sx={{ mb: 2 }}
                />
              )}

              {selectedTransaction.notes && (
                <Typography variant="body2" gutterBottom>
                  Anteckning: {selectedTransaction.notes}
                </Typography>
              )}

              <Divider sx={{ my: 2 }} />

              {/* Attachments Section */}
              <Box>
                <Typography variant="h6" gutterBottom>
                  Kvitton
                </Typography>

                {/* Upload Button */}
                <Box mb={2}>
                  <Input
                    id="file-upload"
                    type="file"
                    inputProps={{
                      accept: 'image/*,application/pdf'
                    }}
                    onChange={handleFileUpload}
                    style={{ display: 'none' }}
                  />
                  <label htmlFor="file-upload">
                    <Button
                      variant="outlined"
                      component="span"
                      startIcon={<AttachFileIcon />}
                      disabled={uploading}
                    >
                      {uploading ? 'Laddar upp...' : 'Bifoga kvitto'}
                    </Button>
                  </label>
                </Box>

                {/* Attachments List */}
                {selectedTransaction.attachments && selectedTransaction.attachments.length > 0 ? (
                  <List dense>
                    {selectedTransaction.attachments.map((attachment) => (
                      <ListItem key={attachment.id} divider>
                        <ListItemIcon>
                          {getFileIcon(attachment.contentType)}
                        </ListItemIcon>
                        <ListItemText
                          primary={attachment.fileName}
                          secondary={`${formatFileSize(attachment.fileSize)} • ${formatDate(attachment.uploadedAt)}`}
                        />
                        <IconButton
                          size="small"
                          onClick={() => handleDownloadAttachment(attachment)}
                        >
                          <DownloadIcon />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteAttachment(attachment)}
                        >
                          <DeleteIcon />
                        </IconButton>
                      </ListItem>
                    ))}
                  </List>
                ) : (
                  <Typography variant="body2" color="textSecondary">
                    Inga bifogade kvitton än.
                  </Typography>
                )}
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>
            Stäng
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Transactions;