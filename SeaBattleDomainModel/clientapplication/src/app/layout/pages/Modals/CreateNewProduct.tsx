import { Formik } from 'formik';
import React, { useState } from 'react'
import { Modal } from 'semantic-ui-react'
import { useStore } from '../../../stores/store';
import ProductModalForm from './ProductModalForm';

interface Props{
    trigger: React.ReactNode
}

function ModalExampleModal(props : Props) {
  const [open, setOpen] = useState(false)
  const {productStore} = useStore();
  const [disable, setDisable] = useState(false);
    
function handleNewProductCreation( values : any,
    setErrors: (errors: import("formik")
               .FormikErrors<{ title: string; price: string; description: string; error: string | null; }>) => void): any {
        if(values.price <= 0){
          var message = "Price has incorrect value";
          console.log(message)
        }
        productStore.createProduct(values)
          .catch(error => setErrors({error: message}));
        setOpen(false);
}

  return (
    <Modal
      onClose={() =>setOpen(false)}
      onOpen={() => setOpen(true)}
      open={open}
      trigger={props.trigger}
    >
      <Modal.Header>Add new product</Modal.Header>
      <Modal.Content>
            <Formik 
                initialValues ={{
                    id: '', 
                    title: '',
                    price: 0,
                    description: '',
                    error: null
            }}
            onSubmit={(initialValues, {setErrors}) => handleNewProductCreation(initialValues, setErrors)}
            >
            {({handleSubmit, isSubmitting, errors}) => (
              <ProductModalForm 
                handleSubmit={handleSubmit} 
                isSubmitting={isSubmitting}
                applyButtonContent={'Create'}
                errors = {errors}
                />
            )}
        </Formik>

      </Modal.Content>
    </Modal>
  )

}

export default ModalExampleModal